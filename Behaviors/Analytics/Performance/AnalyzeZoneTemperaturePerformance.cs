using BrickSchema.Net;

using LisaCore.Behaviors.DataAccess;
using LisaCore.Behaviors.Enums;
using LisaCore.Behaviors.Models;
using LisaCore.MachineLearning.Efficiency.Temperature;


namespace LisaCore.Behaviors.Analytics.Performance
{
    /// <summary>
    /// AnalyzeZonePerformance: Evaluate the HVAC performance for individual zones or areas.
    /// </summary>
    public class AnalyzeZoneTemperaturePerformance : BrickBehavior
    {
        public delegate void AnalyticsResultFunction(List<Result> results);
        private readonly AnalyticsResultFunction _analyticsResultFuntion;
        private readonly double _deadband;
        private int _pollRate;
        private bool _isExecuting;
        private DateTime _lastExecutionTime;

        public AnalyzeZoneTemperaturePerformance(AnalyticsResultFunction analyticsResultCallBackFuntion, double deadband, int intervalMinute = 15, int weight = 1)
            : base(typeof(AnalyzeZoneTemperaturePerformance).Name, BehaviorTypes.Analytics.ToString() , "Zone Temperature Conformance", weight)
        {
            _pollRate = intervalMinute * 60;
            
            _analyticsResultFuntion = analyticsResultCallBackFuntion;
            _deadband = deadband;
            _isExecuting = false;
            _lastExecutionTime = DateTime.Now;
        }

        protected override void Execute()
        {
            if (_lastExecutionTime.AddSeconds(_pollRate) > DateTime.Now || _isExecuting) return; //not time to run or is running already

            _isExecuting = true;
            
           

            try
            {
                List<Result> results = new();
                BrickSchema.Net.Classes.Point? zoneTempPoint = null;
                BrickSchema.Net.Classes.Point? zoneTempSetpoint = null;

                var points = Parent.GetPointEntities();
                foreach (var point in points)
                {
                    var tags = point.GetTags();
                    foreach (var tag in tags)
                    {
                        if (tag.Name.Equals("Zone Air Temperature Sensor"))
                        {
                            zoneTempPoint = point;
                        }
                        else if (tag.Name.Equals("Effective Zone Air Temperature Setpoint"))
                        {
                            zoneTempSetpoint = point;
                        }
                    }
                }
                if (zoneTempPoint != null && zoneTempSetpoint != null)
                {
                    DateTime end = DateTime.Now;
                    DateTime start = end.AddMinutes(-60);
                    var history = zoneTempPoint.Behaviors.FirstOrDefault(x => x.Type.Equals(typeof(HistorizePointInMemory).Name)) as HistorizePointInMemory;
                    if (history != null)
                    {
                        var zoneTempHistory = history.GetHistory(start, end, 1);
                        if (zoneTempHistory.Count > 0)
                        {
                            history = zoneTempSetpoint.Behaviors.FirstOrDefault(x => x.Type.Equals(typeof(HistorizePointInMemory).Name)) as HistorizePointInMemory;
                            if (history != null)
                            {
                                var zoneSetpointHistory = history.GetHistory(start, end, 1);
                                if (zoneSetpointHistory.Count > 0)
                                {
                                    List<TemperatureData> temperatureData = new List<TemperatureData>();
                                    for (int i = 0; i < zoneTempHistory.Count; i++)
                                    {
                                        double setpoint = 0;
                                        if (zoneSetpointHistory.Count > i)
                                        {
                                            setpoint = zoneSetpointHistory[i].Item2;
                                        }
                                        else if (temperatureData.Count > 0)
                                        {
                                            setpoint = temperatureData[temperatureData.Count - 1].Setpoint;
                                        }
                                        temperatureData.Add(new TemperatureData() { Timestamp = zoneTempHistory[i].Item1, Temperature = zoneTempHistory[i].Item2, Setpoint = setpoint });
                                    }

                                    if (temperatureData.Count > 0)
                                    {
                                        try
                                        {
                                            ClearErrors(); //all checks are cleared
                                            var result = new TemperatureEfficiency(_logger).CalculateEfficiency(temperatureData, _deadband);
                                            if (result.Count > 0)
                                            {
                                                Result item = new Result();
                                                item.BehaviorId = Id;
                                                item.BehaviorType = Type;
                                                item.BehaviorName = Name;
                                                item.EntityId = Parent.Id;
                                                item.Value = result[result.Count - 1].Efficiency;
                                                item.Timestamp = result[result.Count - 1].Timestamp;
                                                item.Status = ResultStatusTypes.Success;
                                                List<AnalyticsDataItem> efficiencies = new List<AnalyticsDataItem>();
                                                List<AnalyticsDataItem> deviations = new List<AnalyticsDataItem>();
                                                foreach (var ad in result)
                                                {
                                                    efficiencies.Add(new() { Timestamp = ad.Timestamp, Value = ad.Efficiency });
                                                    deviations.Add(new() { Timestamp = ad.Timestamp, Value = ad.DeviationScore });

                                                }
                                                item.AnalyticsData.Add("Zone Temperature Conformance", efficiencies);
                                                
                                                item.AnalyticsData.Add("Zone Temperature Deviation Score", efficiencies);
                                                results.Add(item);
                                                AddOrUpdateProperty(BrickSchema.Net.EntityProperties.PropertiesEnum.Conformance, item.Value);
                                                AddOrUpdateProperty(BrickSchema.Net.EntityProperties.PropertiesEnum.Deviation, result[result.Count - 1].DeviationScore);
                                            }
                                            else
                                            {
                                                results.Add(new()
                                                {
                                                    BehaviorId = Id,
                                                    BehaviorType = Type,
                                                    BehaviorName = Name,
                                                    EntityId = Parent.Id,
                                                    Value = null,
                                                    Timestamp = DateTime.Now,
                                                    Text = "Analytics result has no value.",
                                                    Status = ResultStatusTypes.Failure

                                                });

                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            results.Add(new()
                                            {
                                                BehaviorId = Id,
                                                BehaviorType = Type,
                                                BehaviorName = Name,
                                                EntityId = Parent.Id,
                                                Value = null,
                                                Timestamp = DateTime.Now,
                                                Text = $"Exception:{ex.Message}",
                                                Status = ResultStatusTypes.Failure

                                            });
                                        }
                                    }

                                }
                                else
                                {
                                    if (NotifyError("NoTempSpHistory"))
                                    {
                                        results.Add(new()
                                        {
                                            BehaviorId = Id,
                                            BehaviorType = Type,
                                            BehaviorName = Name,
                                            EntityId = Parent.Id,
                                            Value = null,
                                            Timestamp = DateTime.Now,
                                            Text = "No Zone Temperature Setpoint history.",
                                            Status = ResultStatusTypes.Skipped

                                        });
                                    }
                                }
                            }
                            else
                            {
                                if (NotifyError("NoTempSpHistorian"))
                                {
                                    results.Add(new()
                                    {
                                        BehaviorId = Id,
                                        BehaviorType = Type,
                                        BehaviorName = Name,
                                        EntityId = Parent.Id,
                                        Value = null,
                                        Timestamp = DateTime.Now,
                                        Text = "Temperature sensor point doesn't have a historian behavior",
                                        Status = ResultStatusTypes.Skipped

                                    });
                                }
                            }
                        }
                        else
                        {
                            if (NotifyError("NoTempHistory"))
                            {
                                results.Add(new()
                                {
                                    BehaviorId = Id,
                                    BehaviorType = Type,
                                    BehaviorName = Name,
                                    EntityId = Parent.Id,
                                    Value = null,
                                    Timestamp = DateTime.Now,
                                    Text = "No Zone Temperature history.",
                                    Status = ResultStatusTypes.Skipped

                                });
                            }
                        }
                    }
                    else
                    {
                        if (NotifyError("NoTempHistorian"))
                        {
                            results.Add(new()
                            {
                                BehaviorId = Id,
                                BehaviorType = Type,
                                BehaviorName = Name,
                                EntityId = Parent.Id,
                                Value = null,
                                Timestamp = DateTime.Now,
                                Text = "Temperature sensor point doesn't have a historian behavior.",
                                Status = ResultStatusTypes.Skipped

                            });
                        }
                    }
                }
                else
                {
                    if (zoneTempPoint == null)
                    {
                        if (NotifyError("zoneTempPoint"))
                        {
                            results.Add(new()
                            {
                                BehaviorId = Id,
                                BehaviorType = Type,
                                BehaviorName = Name,
                                EntityId = Parent.Id,
                                Value = null,
                                Timestamp = DateTime.Now,
                                Text = "Missing point with tag [Zone Air Temperature Sensor]",
                                Status = ResultStatusTypes.Skipped

                            });
                        }
                    }
                    if (zoneTempSetpoint == null)
                    {
                        if (NotifyError("zoneTempSetpoint"))
                        {
                            results.Add(new()
                            {
                                BehaviorId = Id,
                                BehaviorType = Type,
                                BehaviorName = Name,
                                EntityId = Parent.Id,
                                Value = null,
                                Timestamp = DateTime.Now,
                                Text = "Missing point with tag [Effective Zone Air Temperature Setpoint]",
                                Status = ResultStatusTypes.Skipped

                            });
                        }
                    }

                }
                _analyticsResultFuntion(results);
            } catch (Exception ex)
            {
                _isExecuting = false;
                throw;
            }
            _isExecuting = false;
            _lastExecutionTime = DateTime.Now;

        } //end execute
        

        
    }
}
