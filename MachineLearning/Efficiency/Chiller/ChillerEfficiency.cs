using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LisaCore.MachineLearning.Efficiency.Chiller
{
    public class ChillerEfficiency
    {
        // Conversion factor from gallons per minute to gallons per hour
        private const double ConversionFactorGPMtoGPH = 60;

        // Conversion factor from gallons per hour to BTU/hr (for water)
        private const double ConversionFactorGPHtoBTUhr = (8.33 * 60);

        // Conversion factor from kW to BTU/hr
        private const double ConversionFactor = 3412.14;

        // Conversion factor from kW to W
        private const double ConversionFactorKWtoW = 1000;

        internal double CalculateChillerLoad(ChillerData data)
        {

            double temperatureDifference = data.InletTemperature - data.OutletTemperature;
            double flowRateGPH = data.FlowRate * ConversionFactorGPMtoGPH;
            return flowRateGPH * temperatureDifference * ConversionFactorGPHtoBTUhr;
        }

        internal double CalculateCOP(double energyInputKW, double coolingCapacityBTU)
        {
            double EnergyInputBTU = energyInputKW * ConversionFactor;
            return coolingCapacityBTU / EnergyInputBTU;
        }

        internal double CalculateEER(double energyInputKW, double coolingCapacityBTU)
        {
            double EnergyInputW = energyInputKW * ConversionFactorKWtoW;
            return coolingCapacityBTU / EnergyInputW;
        }

        public ChillerEfficiencyResult CalculateEfficiency(ChillerData data, List<ChillerDataDesigned> chillerDesignedData)
        {
            ChillerEfficiencyResult result = (ChillerEfficiencyResult)data;
            result.Load = CalculateChillerLoad(data);
            result.COP = CalculateCOP(data.EnergyInputKW, result.Load);

            //sort the design points
            chillerDesignedData = chillerDesignedData.OrderBy(points => points.Load).ToList();

            // Find the two design points that the current load is between
            ChillerDataDesigned lowerDesignPoint = null;
            ChillerDataDesigned upperDesignPoint = null;

            foreach (ChillerDataDesigned point in chillerDesignedData)
            {
                if (point.Load <= result.Load)
                {
                    lowerDesignPoint = point;
                }
                else if (point.Load > result.Load)
                {
                    upperDesignPoint = point;
                    break;
                }
            }

            // If the current load is outside the range of the design data, use the nearest design point
            if (lowerDesignPoint == null)
            {
                lowerDesignPoint = upperDesignPoint;
            }
            else if (upperDesignPoint == null)
            {
                upperDesignPoint = lowerDesignPoint;
            }

            // Interpolate the COP and EER at the current load
            double interpolatedCOP = Interpolate(lowerDesignPoint.Load, upperDesignPoint.Load, lowerDesignPoint.COP, upperDesignPoint.COP, result.Load);

            result.COP = (result.COP / interpolatedCOP) * 100;
            result.Efficiency = result.COP;
            return result;
        }

        public List<ChillerEfficiencyResult> CalculateEfficiency(List<ChillerData> data, List<ChillerDataDesigned> chillerDesignedData)
        {
            List<ChillerEfficiencyResult> results = new List<ChillerEfficiencyResult>();
            foreach (ChillerData chillerData in data)
            {
                results.Add(CalculateEfficiency(chillerData, chillerDesignedData));
            }
            return results;
        }

        private double Interpolate(double x1, double x2, double y1, double y2, double x)
        {
            return y1 + (x - x1) * (y2 - y1) / (x2 - x1);
        }
    }
}
