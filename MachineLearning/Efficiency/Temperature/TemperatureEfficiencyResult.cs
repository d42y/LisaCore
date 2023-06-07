using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LisaCore.MachineLearning.Efficiency.Temperature
{
    // Define the efficiency output schema
    public class TemperatureEfficiencyResult : TemperatureDataCalculated
    {
        [ColumnName("Score")]
        [Obfuscation(Feature = "renaming", Exclude = true)]
        public double Efficiency { get; set; }
        [Obfuscation(Feature = "renaming", Exclude = true)]
        public double DeviationScore { get; set; }
        [Obfuscation(Feature = "renaming", Exclude = true)]
        public double ViolationScore { get; set; }



        public static TemperatureEfficiencyResult Convert(TemperatureDataCalculated data)
        {
            TemperatureEfficiencyResult result = new();
            result.Temperature = data.Temperature;
            result.Setpoint = data.Setpoint;
            result.Timestamp = data.Timestamp;
            result.DistanceFromSetpoint = data.DistanceFromSetpoint;
            result.TemperatureChangeRate = data.TemperatureChangeRate;

            return result;
        }
    }
}
