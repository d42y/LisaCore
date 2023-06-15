using BrickSchema.Net.Classes.Collection.Loop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LisaCore.MachineLearning.Efficiency.Chiller
{
    public class ChillerPartloadData
    {
        public double CondenserWaterTemperature { get; set; } = 0;
        public double LoadTon { get; set; } = 0;
        public double KwPerTon { get; set; } = 0;
        public bool OptimumLoad { get; set; } = false;
    }
}
