using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LisaCore.MachineLearning.Efficiency.AirFlow
{
    public class AirFlowData
    {
        public DateTime Timestamp { get; set; }
        public double AirFlowCFM { get; set; }
        public double AirFlowSpCFM { get; set; }
    }
}
