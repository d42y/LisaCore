using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LisaCore.MachineLearning.Efficiency.Pump
{
    public class WaterPumpEfficiency
    {
        // Convert pressure to head
        private double PressureToHead(double pressure)
        {
            // A rough conversion from PSI to feet of water is to multiply by 2.31
            return (pressure * 2.31);
        }

        // Convert power from Watts to horsepower
        private double ConvertPowerToHP(double powerInWatts)
        {
            return powerInWatts / 746;
        }

        public PumpEfficiencyResult CalculateEfficiency(PumpData data)
        {
            PumpEfficiencyResult result = (PumpEfficiencyResult)data;


            // Head in feet
            double head = PressureToHead(result.OutletPressure - result.InletPressure);

            // Convert power to horsepower
            double powerInHP = ConvertPowerToHP(result.Power);

            // Water horsepower
            double waterHP = result.FlowRate * head / 3960;

            // Efficiency in percent
            result.Efficiency = (waterHP / powerInHP) * 100;

            return result;
        }

        public List<PumpEfficiencyResult> CalculateEfficiency(List<PumpData> data)
        {
            List<PumpEfficiencyResult> pumpEfficiencyResults = new List<PumpEfficiencyResult>();
            foreach (PumpData dataItem in data)
            {
                pumpEfficiencyResults.Add(CalculateEfficiency(dataItem));
            }

            return pumpEfficiencyResults;
        }
    }
}
