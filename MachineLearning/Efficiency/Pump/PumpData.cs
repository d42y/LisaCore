using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LisaCore.MachineLearning.Efficiency.Pump
{
    public class PumpData
    {
        public DateTime Timestamp { get; set; }
        public double FlowRate { get; set; } // Flow rate in GPM
        public double InletPressure { get; set; } // In PSI
        public double OutletPressure { get; set; } // In PSI
        public double Power { get; set; } // Pump power in Watts
    }
}
