using BrickSchema.Net;
using LisaCore.Behaviors.Enums;
using LisaCore.Behaviors.Models;
using LisaCore.MachineLearning.Efficiency.Chiller;


namespace LisaCore.Behaviors.Analytics.Performance
{
    public class AnalyzeChillerPerformance : BrickBehavior
    {
        public delegate void AnalyticsResultFunction(List<Result> results);
        private readonly AnalyticsResultFunction _analyticsResultFuntion;
        private readonly List<ChillerPartloadData> _partloadData;

        public AnalyzeChillerPerformance(AnalyticsResultFunction analyticsResultCallBackFuntion, List<ChillerPartloadData> partloadData, int intervalMinute = 15, double weight = 1)
            : base(typeof(AnalyzeChillerPerformance).Name, BehaviorTypes.Analytics.ToString(), "Chiller Efficiency", weight)
        {
            AddOrUpdateProperty(BrickSchema.Net.EntityProperties.PropertiesEnum.PollRate, intervalMinute * 60);
            _partloadData = partloadData;
            _analyticsResultFuntion = analyticsResultCallBackFuntion;
        }

        protected override void Execute()
        {
            List<Result> results = new();
            BrickSchema.Net.Classes.Point point = null;



            _analyticsResultFuntion(results);

        }
        

    }
}
