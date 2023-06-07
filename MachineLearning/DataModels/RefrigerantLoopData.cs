using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineLearningLibrary.DataModels
{
    public class RefrigerantLoopData
    {
        public DateTime Timestamp { get; set; }
        public float CompressorPower { get; set; }
        public float EvaporatorLineTemperature { get; set; }
        public float EvaporatorCoilTemperature { get; set; }
        public float CondenserLineTemperature { get; set; }
        public float CondenserCoilTemperature { get; set; }
    }
}
