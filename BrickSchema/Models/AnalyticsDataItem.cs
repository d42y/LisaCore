using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LisaCore.BrickSchema.Models
{
    public class AnalyticsDataItem
    {
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public double? Value { get; set; } = null;
        public string Text { get; set; } = string.Empty;
    }
}
