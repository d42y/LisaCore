using BrickSchema.Net.Classes.Points;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LisaCore.BrickSchema.Models
{
    public class Point
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public double? Value { get; set; } = null;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public PointValueQuality Quality { get; set; } = PointValueQuality.Unknown;
    }
}
