using BrickSchema.Net;
using LisaCore.Behaviors.Models;
using LisaCore.MachineLearning.Efficiency.Chiller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LisaCore.Behaviors.Analytics.Performance
{
    public class AnalyzeChillerPerformance : BrickBehavior
    {
        public delegate void AnalyticsResultFunction(List<Result> results);
        private readonly AnalyticsResultFunction _analyticsResultFuntion;
        private readonly List<ChillerPartloadData> _partloadData;

        public AnalyzeChillerPerformance(AnalyticsResultFunction analyticsResultCallBackFuntion, List<ChillerPartloadData> partloadData, int intervalMinute = 15)
            : base("Chiller Efficiency", typeof(AnalyzeChillerPerformance).Name)
        {
            PollRate = intervalMinute * 60;
            _partloadData = partloadData;
            _analyticsResultFuntion = analyticsResultCallBackFuntion;
        }

        public override void OnTimerTick(object? state)
        {
            List<Result> results = new();
            BrickSchema.Net.Classes.Point point = null;



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
