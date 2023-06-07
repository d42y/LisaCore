using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LisaCore.MachineLearning.Efficiency.Chiller
{
    public class ChillerDataCalculated : ChillerData
    {
        public double Load { get; set; } //Btu
        public double COP { get; set; }
    }
}
