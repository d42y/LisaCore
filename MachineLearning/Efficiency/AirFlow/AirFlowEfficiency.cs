using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LisaCore.MachineLearning.Efficiency.AirFlow
{
    public class AirFlowEfficiency
    {

        public double[] CalculateBestFitLine(List<AirFlowData> data)
        {
            double[] xValues = GenerateTimePeriods(data);
            List<double> _airFlowData = new List<double>();
            foreach (var item in data) { _airFlowData.Add(item.AirFlowCFM); }
            double[] yValues = _airFlowData.ToArray();

            var p = Fit.Line(xValues, yValues);
            double slope = p.Item1;
            double intercept = p.Item2;

            return new double[] { slope, intercept };
        }

        public List<AirFlowEfficiencyResult> CalculateEfficiency(List<AirFlowData> data)
        {
            double[] bestFitLine = CalculateBestFitLine(data);
            List<AirFlowEfficiencyResult> efficiencies = new();

            for (int i = 0; i < data.Count; i++)
            {
                double predictedValue = bestFitLine[0] * i + bestFitLine[1]; // y = mx + c
                double actualValue = data[i].AirFlowSpCFM;
                double percentDeviation = Math.Abs((predictedValue - actualValue) / actualValue);
                AirFlowEfficiencyResult airFlowEfficiencyResult = new AirFlowEfficiencyResult();
                airFlowEfficiencyResult.AirFlowCFM = data[i].AirFlowCFM;
                airFlowEfficiencyResult.AirFlowSpCFM = data[i].AirFlowSpCFM;
                airFlowEfficiencyResult.Timestamp = data[i].Timestamp;
                airFlowEfficiencyResult.Slope = bestFitLine[0];
                airFlowEfficiencyResult.Intercept = bestFitLine[1];
                if (percentDeviation <= 0.1)
                {
                    airFlowEfficiencyResult.Efficiency = 100 - (percentDeviation * percentDeviation);
                }
                else if (percentDeviation <= 0.3)
                    airFlowEfficiencyResult.Efficiency = 90 - (percentDeviation - 0.1) / 0.2 * 90;
                else
                    airFlowEfficiencyResult.Efficiency = 0;

                efficiencies.Add(airFlowEfficiencyResult);
            }

            return efficiencies;
        }

        private double[] GenerateTimePeriods(List<AirFlowData> data)
        {
            double[] timePeriods = new double[data.Count];
            for (int i = 0; i < data.Count; i++)
                timePeriods[i] = i;

            return timePeriods;
        }
    }
}
