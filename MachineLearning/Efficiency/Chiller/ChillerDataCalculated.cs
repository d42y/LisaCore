using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LisaCore.MachineLearning.Efficiency.Chiller
{
    public class ChillerDataCalculated : ChillerData
    {
        public double Btu { get; set; } //Btu
        public double KwTon { get; set; } //kw per ton
        public double COP { get; set; }
    }
}
