using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LisaCore.MachineLearning.Efficiency.Temperature
{
    public class TemperatureDataCalculated : TemperatureData
    {
        [Obfuscation(Feature = "renaming", Exclude = true)]
        public double TemperatureChangeRate { get; set; }
        [Obfuscation(Feature = "renaming", Exclude = true)]
        public double DistanceFromSetpoint { get; set; }

        public static List<TemperatureDataCalculated> FormatTemperatureData(List<TemperatureData> rawTemperatureData)
        {
            if (rawTemperatureData.Count == 0) return new();
            List<TemperatureDataCalculated> smoothedTemperatureData = new List<TemperatureDataCalculated>();
            int i = 0;

            while (i < rawTemperatureData.Count - 1)
            {
                double currentTemperature = rawTemperatureData[i].Temperature;
                int steps = 1;

                // Determine the number of steps until the next value change.
                while (i + steps < rawTemperatureData.Count && rawTemperatureData[i + steps].Temperature == currentTemperature)
                {
                    steps++;
                }



                if (i + steps < rawTemperatureData.Count)
                {
                    double nextTemperature = rawTemperatureData[i + steps].Temperature;
                    double stepDifference = (nextTemperature - currentTemperature) / steps;

                    for (int j = 0; j < steps; j++)
                    {
                        var tempData = Convert(rawTemperatureData[i + j]);
                        tempData.TemperatureChangeRate = (float)(stepDifference * j);
                        tempData.Temperature = (float)(currentTemperature + stepDifference * j);
                        smoothedTemperatureData.Add(tempData);
                    }
                }
                else
                {
                    var tempData = Convert(rawTemperatureData[i]);
                    tempData.TemperatureChangeRate = 0;
                    smoothedTemperatureData.Add(tempData);
                }

                i += steps;
            }

            var lastData = Convert(rawTemperatureData[rawTemperatureData.Count - 1]);
            lastData.TemperatureChangeRate = 0;
            smoothedTemperatureData.Add(lastData);

            return smoothedTemperatureData;
        }

        public static TemperatureDataCalculated Convert(TemperatureData data)
        {
            TemperatureDataCalculated fdata = new TemperatureDataCalculated();
            fdata.Temperature = data.Temperature;
            fdata.Setpoint = data.Setpoint;
            fdata.Timestamp = data.Timestamp;
            return fdata;
        }
    }
}
