using MachineLearningLibrary.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineLearningLibrary.Detect.RefrigerantLoop
{
    public class FailuresDetection
    {

        private List<RefrigerantLoopData> LoadData(string filePath)
        {
            List<RefrigerantLoopData> dataList = new List<RefrigerantLoopData>();
            using (var reader = new StreamReader(filePath))
            {
                // Read the header line
                reader.ReadLine();

                while (!reader.EndOfStream)
                {
                    // Read each data line and parse the values
                    var line = reader.ReadLine();
                    var values = line.Split(',');

                    var data = new RefrigerantLoopData();
                    data.Timestamp = DateTime.Parse(values[0]);
                    data.CompressorPower = float.Parse(values[1]);
                    data.EvaporatorLineTemperature = float.Parse(values[2]);
                    data.EvaporatorCoilTemperature = float.Parse(values[3]);
                    data.CondenserLineTemperature = float.Parse(values[4]);
                    data.CondenserCoilTemperature = float.Parse(values[5]);
                    dataList.Add(data);
                }
            }
            return dataList;
        }


        public void GetFailures()
        {

            // Load the historical data from a CSV file
            var dataList = LoadData("refrigerant_loop_data.csv");

            // Analyze the data for issues
            var issueList = new List<string>();
            var powerThreshold = 10.0f;
            var coilTemperatureThreshold = 5.0f;
            var expectedEvaporatorTemperatureRange = new Tuple<float, float>(-45.0f, 10.0f);
            var expectedCondenserTemperatureRange = new Tuple<float, float>(15.0f, 50.0f);
            var expectedSuperheat = 5.0f;
            var expectedSubcooling = 10.0f;
            var refrigerantType = "R-290";
            var evaporatorCoilDesign = "flooded";
            var condenserCoilDesign = "finned tube";

            for (var i = 1; i < dataList.Count; i++)
            {
                var previousData = dataList[i - 1];
                var currentData = dataList[i];
                var timeDelta = (currentData.Timestamp - previousData.Timestamp).TotalSeconds;
                var compressorPowerChange = Math.Abs(currentData.CompressorPower - previousData.CompressorPower) / timeDelta;
                var evaporatorTemperature = currentData.EvaporatorLineTemperature - currentData.EvaporatorCoilTemperature;
                var condenserTemperature = currentData.CondenserLineTemperature - currentData.CondenserCoilTemperature;
                var evaporatorPressure = EstimatePressure(refrigerantType, evaporatorCoilDesign, currentData.EvaporatorCoilTemperature);
                var condenserPressure = EstimatePressure(refrigerantType, condenserCoilDesign, currentData.CondenserCoilTemperature);
                var superheat = currentData.EvaporatorLineTemperature - SaturationTemperature(evaporatorPressure);
                var subcooling = SaturationTemperature(condenserPressure) - currentData.CondenserLineTemperature;

                if (compressorPowerChange > powerThreshold)
                {
                    issueList.Add($"Compressor power is changing too rapidly: previous power was {previousData.CompressorPower:F2}, current power is {currentData.CompressorPower:F2}");
                }

                if (Math.Abs(currentData.EvaporatorLineTemperature - currentData.EvaporatorCoilTemperature) > coilTemperatureThreshold)
                {
                    issueList.Add($"Evaporator coil temperature is too high: coil temperature is {currentData.EvaporatorCoilTemperature:F2} and line temperature is {currentData.EvaporatorLineTemperature:F2}");
                }

                if (Math.Abs(currentData.CondenserLineTemperature - currentData.CondenserCoilTemperature) > coilTemperatureThreshold)
                {
                    issueList.Add($"Condenser coil temperature is too high: coil temperature is {currentData.CondenserCoilTemperature:F2} and line temperature is {currentData.CondenserLineTemperature:F2}");
                }

                if (evaporatorTemperature < expectedEvaporatorTemperatureRange.Item1 || evaporatorTemperature > expectedEvaporatorTemperatureRange.Item2)
                {
                    issueList.Add($"Evaporator temperature is outside of expected range: temperature is {evaporatorTemperature:F2} (expected range is {expectedEvaporatorTemperatureRange.Item1:F2} to {expectedEvaporatorTemperatureRange.Item2:F2})");
                }
                if (condenserTemperature < expectedCondenserTemperatureRange.Item1 || condenserTemperature > expectedCondenserTemperatureRange.Item2)
                {
                    issueList.Add($"Condenser temperature is outside of expected range: temperature is {condenserTemperature:F2} (expected range is {expectedCondenserTemperatureRange.Item1:F2} to {expectedCondenserTemperatureRange.Item2:F2})");
                }

                if (Math.Abs(superheat - expectedSuperheat) > expectedSuperheat)
                {
                    issueList.Add($"Superheat is outside of expected range: superheat is {superheat:F2} (expected range is {expectedSuperheat:F2} +/- {expectedSuperheat:F2})");
                }

                if (Math.Abs(subcooling - expectedSubcooling) > expectedSubcooling)
                {
                    issueList.Add($"Subcooling is outside of expected range: subcooling is {subcooling:F2} (expected range is {expectedSubcooling:F2} +/- {expectedSubcooling:F2})");
                }

                // Check refrigerant charge level
                var expectedChargeLevel = "Normal";
                var saturationTemperature = SaturationTemperature(evaporatorPressure);
                var actualChargeLevel = EstimateChargeLevel(refrigerantType, evaporatorCoilDesign, saturationTemperature, currentData.EvaporatorCoilTemperature, currentData.CondenserCoilTemperature, evaporatorPressure, condenserPressure);
                if (actualChargeLevel != expectedChargeLevel)
                {
                    issueList.Add($"Refrigerant charge level is {actualChargeLevel}: expected charge level is {expectedChargeLevel}");
                }
            }

            // Print out any issues
            if (issueList.Count > 0)
            {
                Console.WriteLine($"The following issues were detected:");
                foreach (var issue in issueList)
                {
                    Console.WriteLine($"- {issue}");
                }
            }
            else
            {
                Console.WriteLine("No issues detected.");
            }



        }

        static float SaturationTemperature(float pressure)
        {
            // This function uses a refrigerant property library to lookup the saturation temperature for the given pressure
            // The refrigerant property library is not included in this example code, but there are many commercial and open-source libraries available

            // Here's an example of how you might implement this function using a lookup table:
            var pressureTable = new Dictionary<float, float>
            {
                { 101.3f, 0.0f },
                { 200.0f, 2.0f },
                { 400.0f, 6.0f },
                { 600.0f, 10.0f },
                { 800.0f, 14.0f },
                { 1000.0f, 18.0f },
                // add more rows for additional pressures
            };

            if (pressureTable.ContainsKey(pressure))
            {
                return pressureTable[pressure];
            }
            else
            {
                // perform interpolation between two closest values in the table
                var pressures = pressureTable.Keys.OrderBy(p => Math.Abs(p - pressure)).Take(2).ToList();
                var temperatures = pressureTable.Values.OrderBy(t => Math.Abs(t - pressureTable[pressure])).Take(2).ToList();
                var slope = (temperatures[1] - temperatures[0]) / (pressures[1] - pressures[0]);
                return temperatures[0] + slope * (pressure - pressures[0]);
            }
        }

        static float EstimatePressure(string refrigerantType, string coilDesign, float coilTemperature)
        {
            // This function uses a refrigerant property library and heat transfer models to estimate the refrigerant pressure at the given coil temperature
            // The refrigerant property library and heat transfer models are not included in this example code, but there are many commercial and open-source libraries available
            // For the purposes of this example code, we'll just assume that the pressure is linearly proportional to the temperature

            // Get the refrigerant properties
            var refrigerantProperties = GetRefrigerantProperties(refrigerantType);

            // Estimate the saturation temperature and pressure at the given coil temperature
            var saturationTemperature = SaturationTemperature(coilTemperature);
            var saturationPressure = refrigerantProperties.GetSaturationPressure(saturationTemperature);

            // Estimate the pressure drop across the coil based on the coil design
            var pressureDrop = 0.0f;
            if (coilDesign == "flooded")
            {
                pressureDrop = 0.01f * (saturationPressure / refrigerantProperties.CriticalPressure) * (coilTemperature / saturationTemperature - 1.0f);
            }
            else if (coilDesign == "finned tube")
            {
                pressureDrop = (float)(0.01f * (saturationPressure / refrigerantProperties.CriticalPressure) * (coilTemperature / saturationTemperature - 1.0f) * (1.0f - 0.5f * Math.Pow(saturationTemperature / coilTemperature, 2)));
            }
            else if (coilDesign == "microchannel")
            {
                pressureDrop = (float)(0.01f * (saturationPressure / refrigerantProperties.CriticalPressure) * (coilTemperature / saturationTemperature - 1.0f) * (1.0f - 0.2f * Math.Pow(saturationTemperature / coilTemperature, 2)));
            }

            return saturationPressure - pressureDrop;
        }

        static string EstimateChargeLevel(string refrigerantType, string evaporatorCoilDesign, float saturationTemperature, float evaporatorCoilTemperature, float condenserCoilTemperature, float evaporatorPressure, float condenserPressure)
        {
            // This function uses a combination of refrigerant property analysis and heat transfer modeling to estimate the refrigerant charge level
            // The refrigerant property library and heat transfer models are not included in this example code, but there are many commercial and open-source libraries available
            // For the purposes of this example code, we'll just assume that the charge level is normal if the evaporator and condenser subcooling are both within a certain range

            // Get the refrigerant properties
            var refrigerantProperties = GetRefrigerantProperties(refrigerantType);

            // Estimate the saturation pressure and temperature at the evaporator coil temperature
            var evaporatorSaturationPressure = refrigerantProperties.GetSaturationPressure(evaporatorCoilTemperature);
            var evaporatorSaturationTemperature = SaturationTemperature(evaporatorSaturationPressure);

            // Estimate the saturation pressure and temperature at the condenser coil temperature
            var condenserSaturationPressure = refrigerantProperties.GetSaturationPressure(condenserCoilTemperature);
            var condenserSaturationTemperature = SaturationTemperature(condenserSaturationPressure);

            // Estimate the evaporator subcooling
            var evaporatorSubcooling = saturationTemperature - evaporatorSaturationTemperature;

            // Estimate the condenser subcooling
            var condenserSubcooling = condenserSaturationTemperature - condenserCoilTemperature;

            if (evaporatorSubcooling >= 5.0f && condenserSubcooling >= 5.0f)
            {
                return "Normal";
            }
            else if (evaporatorSubcooling < 5.0f)
            {
                return "Undercharged";
            }
            else
            {
                return "Overcharged";
            }
        }

        static RefrigerantProperties GetRefrigerantProperties(string refrigerantType)
        {
            // This function returns the refrigerant properties for the given refrigerant type
            // The refrigerant property library is not included in this example code, but there are many commercial and open-source libraries available

            // Here's an example of how you might implement this function:
            if (refrigerantType == "R-290")
            {
                return new RefrigerantProperties { CriticalTemperature = 133.4f, CriticalPressure = 4.25f, MolecularWeight = 44.1f };
            }
            else if (refrigerantType == "R-404A")
            {
                return new RefrigerantProperties { CriticalTemperature = 72.1f, CriticalPressure = 3.74f, MolecularWeight = 97.6f };
            }
            else
            {
                throw new ArgumentException($"Unknown refrigerant type: {refrigerantType}");
            }
        }

        class RefrigerantProperties
        {
            public float CriticalTemperature { get; set; }
            public float CriticalPressure { get; set; }
            public float MolecularWeight { get; set; }

            public float GetSaturationPressure(float temperature)
            {
                // This function returns the saturation pressure for the given temperature
                // The refrigerant property library is not included in this example code, but there are many commercial and open-source libraries available

                // Here's an example of how you might implement this function using the Antoine equation:
                var A = 10.786;
                var B = -2106.8;
                var C = -140.67;
                var pressure = Math.Pow(10, A + B / (C + temperature));
                return (float)pressure;
            }
        }

    }
}
