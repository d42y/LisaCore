using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LisaCore.MachineLearning.Efficiency.Chiller
{
    public class ChillerData
    {
        public DateTime Timestamp { get; set; }
        public double FlowRate { get; set; } // Flow rate in gallons per minute (GPM)
        public double InletTemperature { get; set; } // Inlet water temperature in degrees Fahrenheit
        public double OutletTemperature { get; set; } // Outlet water temperature in degrees Fahrenheit
        public double EnergyInputKW { get; set; }
    }
}
