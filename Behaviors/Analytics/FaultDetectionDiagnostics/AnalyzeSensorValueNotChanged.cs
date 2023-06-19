using BrickSchema.Net;
using BrickSchema.Net.Classes;
using LisaCore.Behaviors.Analytics.Performance;
using LisaCore.Behaviors.Enums;
using LisaCore.Behaviors.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LisaCore.Behaviors.Analytics.FaultDetectionDiagnostics
{
    public class AnalyzeSensorValueNotChanged : BrickBehavior
    {
        public delegate void AnalyticsResultFunction(List<Result> results);
        private readonly AnalyticsResultFunction _analyticsResultFuntion;
        private readonly int _thresholdMinutes;
        private int _pollRate;
        private bool _isExecuting;
        private DateTime _lastExecutionTime;
        public AnalyzeSensorValueNotChanged(AnalyticsResultFunction analyticsResultCallBackFuntion, int thresholdMinutes = 480, double weight = 0.5) :
            base(typeof(AnalyzeZoneTemperaturePerformance).Name, BehaviorTypes.Analytics.ToString(), "Sensor Value Update Conformance", weight)
        {
            _pollRate = thresholdMinutes * 6; //10% of threshold
            _isExecuting = false;
            _lastExecutionTime = DateTime.Now;
            _analyticsResultFuntion = analyticsResultCallBackFuntion;
            _thresholdMinutes = thresholdMinutes;
        }

        protected override void Execute()
        {
            if (_lastExecutionTime.AddSeconds(_pollRate) > DateTime.Now || _isExecuting) { return; }
            _isExecuting = true;
            try
            {
                if (Parent is BrickSchema.Net.Classes.Point)
                {
                    ClearErrors();
                    List<Result> results = new();

                    var point = Parent as BrickSchema.Net.Classes.Point;
                    if (point?.Value.HasValue??false)
                    {
                        var (conformance, deviation)  = CalculateConformance(point.Timestamp, _thresholdMinutes);
                        Result item = new Result();
                        item.BehaviorId = Id;
                        item.BehaviorType = Type;
                        item.BehaviorName = Name;
                        item.EntityId = Parent.Id;
                        item.Value = conformance;
                        item.Timestamp = DateTime.Now;
                        item.Status = ResultStatusTypes.Success;
                        List<AnalyticsDataItem> efficiencies = new List<AnalyticsDataItem>();
                        List<AnalyticsDataItem> deviations = new List<AnalyticsDataItem>();

                        efficiencies.Add(new() { Timestamp = item.Timestamp, Value = conformance });
                        deviations.Add(new() { Timestamp = item.Timestamp, Value = deviation });


                        item.AnalyticsData.Add("Sensor Value Not Changed Conformance", efficiencies);
                        item.AnalyticsData.Add("Sensor Value Not Changed Diviation Seconds", deviations);
                        results.Add(item);

                        AddOrUpdateProperty(BrickSchema.Net.EntityProperties.PropertiesEnum.Conformance, item.Value);
                        AddOrUpdateProperty(BrickSchema.Net.EntityProperties.PropertiesEnum.Deviation, deviation);
                    } else
                    {
                        if (NotifyError("NoPointValue"))
                        {
                            results.Add(new()
                            {
                                BehaviorId = Id,
                                BehaviorType = Type,
                                BehaviorName = Name,
                                EntityId = Parent.Id,
                                Value = null,
                                Timestamp = DateTime.Now,
                                Text = "Sensor Value is Null.",
                                Status = ResultStatusTypes.Skipped

                            });
                        }
                    }
                    _analyticsResultFuntion(results);
                }

            }
            catch (Exception ex)
            {
                _isExecuting = false;
                throw;
            }
            _isExecuting = false;
            _lastExecutionTime = DateTime.Now;

        }

        private static (double, double) CalculateConformance(DateTime valueTimeStamp, int thresholdInMinutes)
        {
            // Convert threshold to seconds
            int threshold = thresholdInMinutes * 60;

            // Calculate elapsed time in seconds
            double elapsedTime = (DateTime.Now - valueTimeStamp).TotalSeconds;

            // If elapsed time is more than threshold, conformance is 0
            if (elapsedTime >= threshold)
            {
                return (0, elapsedTime);
            }
            // If elapsed time is within 10 seconds, conformance is 100
            else if (elapsedTime <= 10)
            {
                return (100, elapsedTime);
            }
            else
            {
                // Define the "high conformance" time as 10% of the threshold
                double highConformanceTime = threshold * 0.10;

                // If elapsed time is within the "high conformance" time, conformance is 100
                if (elapsedTime <= highConformanceTime)
                {
                    return (100, elapsedTime);
                }
                else
                {
                    // Calculate conformance based on linear interpolation between highConformanceTime and threshold
                    double remainingTime = elapsedTime - highConformanceTime;
                    double remainingThreshold = threshold - highConformanceTime;

                    return (100 - (remainingTime / remainingThreshold) * 100, elapsedTime);
                }
            }
        }
    }
}
