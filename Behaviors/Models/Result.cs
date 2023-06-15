using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LisaCore.Behaviors.Models
{
    public class Result
    {
        public string EntityId { get; set; } = string.Empty;
        public string BehaviorType { get; set; } = string.Empty;
        public string BehaviorId { get; set; } = string.Empty;
        public string BehaviorName { get; set; } = string.Empty;
        public double? Value { get; set; } = null;
        public string Text { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public ResultStatusesEnum Status { get; set; } = ResultStatusesEnum.Skipped;
        public Dictionary<string, List<AnalyticsDataItem>> AnalyticsData { get; set; } = new Dictionary<string, List<AnalyticsDataItem>>();
    }
}
