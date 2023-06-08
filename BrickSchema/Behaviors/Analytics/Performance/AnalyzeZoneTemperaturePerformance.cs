using BrickSchema.Net.Behaviors.DataAccess;
using BrickSchema.Net.Classes;
using BrickSchema.Net.Classes.Points;
using BrickSchema.Net.Shapes;
using Google.Protobuf.WellKnownTypes;
using LisaCore.BrickSchema.Models;
using LisaCore.MachineLearning.Efficiency.Temperature;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickSchema.Net.Behaviors.Analytics.Performance
{
    /// <summary>
    /// AnalyzeZonePerformance: Evaluate the HVAC performance for individual zones or areas.
    /// </summary>
    public class AnalyzeZoneTemperaturePerformance : BrickBehavior
    {
        private const string NAME = "Zone Temperature Efficiency";
        public delegate void AnalyticsResultFunction(List<LisaCore.BrickSchema.Models.Result> results);
        private readonly AnalyticsResultFunction _analyticsResultFuntion;
        private readonly double _deadband;
        private Dictionary<string, DateTime> _errorDatetime = new Dictionary<string, DateTime>();
        
        public AnalyzeZoneTemperaturePerformance(AnalyticsResultFunction analyticsResultCallBackFuntion, double deadband, int intervalMinute = 15) 
            : base (NAME, typeof(AnalyzeZoneTemperaturePerformance).Name)
        {
            PollRate = intervalMinute * 60;
            _analyticsResultFuntion = analyticsResultCallBackFuntion;
            _deadband = deadband;
        }

        public override void OnTimerTick(object? state)
        {
            List<LisaCore.BrickSchema.Models.Result> results = new();
            Classes.Point? zoneTempPoint = null;
            Classes.Point? zoneTempSetpoint = null;

            var points = Parent.GetPointEntities();
            foreach ( var point in points )
            {
                var tags = point.GetTags();
                foreach (var tag in tags )
                {
                    if (tag.Name.Equals("Zone Air Temperature Sensor"))
                    {
                        zoneTempPoint = point;
                    } else if (tag.Name.Equals("Effective Zone Air Temperature Setpoint"))
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
                                    } else if (temperatureData.Count > 0)
                                    {
                                        setpoint = temperatureData[temperatureData.Count - 1].Setpoint;
                                    }
                                    temperatureData.Add(new TemperatureData() { Timestamp = zoneTempHistory[i].Item1, Temperature = zoneTempHistory[i].Item2, Setpoint = setpoint});
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
                                            item.Status = LisaCore.BrickSchema.Models.ResultStatusesEnum.Success;
                                            List<AnalyticsDataItem> efficiencies = new List<AnalyticsDataItem>();
                                            List<AnalyticsDataItem> deviations = new List<AnalyticsDataItem>();
                                            foreach (var ad in result)
                                            {
                                                efficiencies.Add(new() { Timestamp = ad.Timestamp, Value = ad.Efficiency });
                                                deviations.Add(new() { Timestamp = ad.Timestamp, Value = ad.DeviationScore });

                                            }
                                            item.AnalyticsData.Add("Zone Temperature Efficiency", efficiencies);
                                            item.AnalyticsData.Add("Zone Temperature Deviation Score", efficiencies);
                                            results.Add(item);

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
                                                Status = LisaCore.BrickSchema.Models.ResultStatusesEnum.Failure

                                            });

                                        }
                                    } catch (Exception ex)
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
                                            Status = LisaCore.BrickSchema.Models.ResultStatusesEnum.Failure

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
                                        Status = LisaCore.BrickSchema.Models.ResultStatusesEnum.Skipped

                                    });
                                }
                            }
                        } else
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
                                    Status = LisaCore.BrickSchema.Models.ResultStatusesEnum.Skipped

                                });
                            }
                        }
                    } else
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
                                Status = LisaCore.BrickSchema.Models.ResultStatusesEnum.Skipped

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
                            Status = LisaCore.BrickSchema.Models.ResultStatusesEnum.Skipped

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
                            Status = LisaCore.BrickSchema.Models.ResultStatusesEnum.Skipped

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
                            Status = LisaCore.BrickSchema.Models.ResultStatusesEnum.Skipped

                        });
                    }
                }
                
            }
            _analyticsResultFuntion(results);

        }
        public override void Start()
        {
            // Convert pollRate from seconds to milliseconds, as required by Timer
            int pollRateMilliseconds = PollRate * 1000;

            // Create and start the timer
            BehaviorTimer = new Timer(OnTimerTick, null, pollRateMilliseconds, pollRateMilliseconds);
        }

        /// <summary>
        /// Optionally, you might want a method to stop the timer
        /// </summary>
        public override void Stop()
        {
            BehaviorTimer?.Dispose();
        }
    }
}
