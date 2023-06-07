using BrickSchema.Net.Classes.Points;
using System.ComponentModel.DataAnnotations;

namespace BrickSchema.Net.Behaviors.DataAccess
{
    internal class PointHistory
    {
        [Key]
        public int Id { get; set; }
        public string Guid { get; set; } = System.Guid.NewGuid().ToString();
        public string PointId { get; set; } //point id
        public DateTime StartTimestamp { get; set; }
        public DateTime EndTimestamp { get; set; }
        public List<double> Values { get; set; } = new();
        public List<double> Intervals { get; set; } = new(); //second
        public List<PointValueQuality> Qualities { get; set; } = new();
        public bool Full { get; set; } = false;
    }
}
