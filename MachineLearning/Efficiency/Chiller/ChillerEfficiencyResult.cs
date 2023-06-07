using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LisaCore.MachineLearning.Efficiency.Chiller
{
    public class ChillerEfficiencyResult : ChillerDataCalculated
    {
        public double Efficiency { get; set; }
        public double COP { get; set; }
    }
}
