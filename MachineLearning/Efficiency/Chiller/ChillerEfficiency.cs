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

        /*
            This function calculates the load (in BTUs per hour) on a chiller given certain parameters.

            @param data: An instance of the ChillerData class. This class should contain the following properties:
                - InletTemperature: The temperature (in degrees) of the fluid entering the chiller.
                - OutletTemperature: The temperature (in degrees) of the fluid exiting the chiller.
                - FlowRate: The rate (in gallons per minute) at which fluid is flowing through the chiller.

            The function uses these parameters to calculate the load on the chiller as follows:
            1. It calculates the temperature difference between the fluid entering and exiting the chiller.
            2. It converts the flow rate from gallons per minute to gallons per hour using a predefined conversion factor.
            3. It multiplies the flow rate (in gallons per hour), the temperature difference, and a predefined conversion factor to calculate the chiller load in BTUs per hour.

            @returns: The calculated chiller load (in BTUs per hour).
        */
        internal double CalculateChillerLoad(ChillerData data)
        {

            double temperatureDifference = data.InletTemperature - data.OutletTemperature; // Calculate the temperature difference between the fluid entering and exiting the chiller.
            double flowRateGPH = data.FlowRate * ConversionFactorGPMtoGPH; // Convert the flow rate from gallons per minute to gallons per hour using the predefined conversion factor.
            return flowRateGPH * temperatureDifference * ConversionFactorGPHtoBTUhr; // Return the calculated chiller load (in BTUs per hour).
        }

        /*
            This function converts power from BTUs per hour to kilowatts per ton.

            @param btusPerHour: The power in BTUs per hour.

            One ton of cooling is approximately equal to 12,000 BTUs per hour, and one BTU per hour is approximately equal to 0.000293071 kilowatts.
            Therefore, to convert from BTUs per hour to kilowatts per ton, we first convert the power from BTUs per hour to kilowatts, then divide by the number of BTUs in one ton.

            @returns: The power in kilowatts per ton.
        */
        public double ConvertBTUhrToKWPerTon(double btusPerHour)
        {
            double btuToKWConversionFactor = 0.000293071; // Conversion factor from BTUs per hour to kilowatts
            double btusPerTon = 12000; // Number of BTUs in one ton

            double kW = btusPerHour * btuToKWConversionFactor; // Convert the power from BTUs per hour to kilowatts
            double kWPerTon = kW / btusPerTon; // Convert the power to kilowatts per ton

            return kWPerTon; // Return the power in kilowatts per ton
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

        public ChillerEfficiencyResult CalculateEfficiency(ChillerData data, List<ChillerPartloadData> partloadDatas)
        {
            ChillerEfficiencyResult result = (ChillerEfficiencyResult)data;
            result.Btu = CalculateChillerLoad(data);
            result.KwTon = ConvertBTUhrToKWPerTon(result.Btu);
            result.COP = CalculateCOP(data.EnergyInputKW, result.Btu);

            //sort the design points
            partloadDatas = partloadDatas.OrderBy(points => points.LoadTon).ToList();

            // Find the two design points that the current load is between
            ChillerPartloadData lowerDesignPoint = null;
            ChillerPartloadData upperDesignPoint = null;

            foreach (ChillerPartloadData point in partloadDatas)
            {
                
                if (point.CondenserWaterTemperature <= result.CondenserWaterTemperature)
                {
                    lowerDesignPoint = point;
                }
                else if (point.LoadTon > result.CondenserWaterTemperature)
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

            //Interpolate the kwton at current condending water temperature
            var loadTon = result.KwTon / result.EnergyInputKW;
            double designKwTon = InterpolateKWTon(result.CondenserWaterTemperature, loadTon, partloadDatas);

            result.Efficiency = CalculateChillerEfficiency(result.COP, designKwTon, result.KwTon);
            return result;
        }

        public List<ChillerEfficiencyResult> CalculateEfficiency(List<ChillerData> data, List<ChillerPartloadData> partloadDatas)
        {
            List<ChillerEfficiencyResult> results = new List<ChillerEfficiencyResult>();
            foreach (ChillerData chillerData in data)
            {
                results.Add(CalculateEfficiency(chillerData, partloadDatas));
            }
            return results;
        }

        /*
            This function calculates the efficiency of a chiller.

            @param cop: The Coefficient of Performance (COP) of the chiller, which is the ratio of the cooling capacity to the power input.
            @param designKWPerTon: The design energy efficiency of the chiller in kilowatts per ton.
            @param runtimeKWPerTon: The actual energy consumption of the chiller in kilowatts per ton during runtime.

            The efficiency of a chiller is calculated as the ratio of its design energy efficiency to its actual energy efficiency. The COP is used to normalize this ratio.

            @returns: The efficiency of the chiller.
        */
        public double CalculateChillerEfficiency(double cop, double designKWPerTon, double runtimeKWPerTon)
        {
            // Check for zero to prevent division by zero.
            if (runtimeKWPerTon == 0)
            {
                throw new ArgumentException("Runtime kW/ton cannot be zero");
            }

            // Calculate the efficiency.
            double efficiency = (cop * designKWPerTon) / runtimeKWPerTon;

            return efficiency;
        }


        private double Interpolate(double x1, double x2, double y1, double y2, double x)
        {
            return y1 + (x - x1) * (y2 - y1) / (x2 - x1);
        }

        /*
            This function performs bilinear interpolation to estimate the KW value for a given Temp and Load.

            @param temp: The temperature value for which to estimate the KW.
            @param load: The load value for which to estimate the KW.
            @param dataPoints: A list of data points, each of which includes a Temp, Load, and KW value.

            Bilinear interpolation is performed by linearly interpolating the KW values along both the Temp and Load dimensions and then averaging the results.

            @returns: The estimated KW value for the given Temp and Load.
        */
        public double InterpolateKWTon(double condenserWaterTemp, double loadTon, List<ChillerPartloadData> partloadDatas)
        {
            // First, sort the data points by Temp and Load.
            var sortedDataPoints = partloadDatas.OrderBy(dp => dp.CondenserWaterTemperature).ThenBy(dp => dp.LoadTon).ToList();

            // Find the data points that are closest to the given Temp and Load.
            var lowerTempDataPoint = sortedDataPoints.LastOrDefault(dp => dp.CondenserWaterTemperature <= condenserWaterTemp);
            var upperTempDataPoint = sortedDataPoints.FirstOrDefault(dp => dp.CondenserWaterTemperature >= condenserWaterTemp);
            var lowerLoadDataPoint = sortedDataPoints.LastOrDefault(dp => dp.LoadTon <= loadTon);
            var upperLoadDataPoint = sortedDataPoints.FirstOrDefault(dp => dp.LoadTon >= loadTon);

            // Perform linear interpolation along the Temp dimension.
            double tempInterpolatedKW;
            if (lowerTempDataPoint != null && upperTempDataPoint != null)
            {
                tempInterpolatedKW = lowerTempDataPoint.KwPerTon + (upperTempDataPoint.KwPerTon - lowerTempDataPoint.KwPerTon) * ((condenserWaterTemp - lowerTempDataPoint.CondenserWaterTemperature) / (upperTempDataPoint.CondenserWaterTemperature - lowerTempDataPoint.CondenserWaterTemperature));
            }
            else if (lowerTempDataPoint != null)
            {
                tempInterpolatedKW = lowerTempDataPoint.KwPerTon;
            }
            else
            {
                tempInterpolatedKW = upperTempDataPoint.KwPerTon;
            }

            // Perform linear interpolation along the Load dimension.
            double loadInterpolatedKW;
            if (lowerLoadDataPoint != null && upperLoadDataPoint != null)
            {
                loadInterpolatedKW = lowerLoadDataPoint.KwPerTon + (upperLoadDataPoint.KwPerTon - lowerLoadDataPoint.KwPerTon) * ((loadTon - lowerLoadDataPoint.LoadTon) / (upperLoadDataPoint.LoadTon - lowerLoadDataPoint.LoadTon));
            }
            else if (lowerLoadDataPoint != null)
            {
                loadInterpolatedKW = lowerLoadDataPoint.KwPerTon;
            }
            else
            {
                loadInterpolatedKW = upperLoadDataPoint.KwPerTon;
            }

            // Return the average of the two interpolated KW values.
            return (tempInterpolatedKW + loadInterpolatedKW) / 2;
        }

    }
}
