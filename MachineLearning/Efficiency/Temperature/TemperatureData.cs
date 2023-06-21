using MachineLearningLibrary.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LisaCore.MachineLearning.Efficiency.Temperature
{

    public class TemperatureData
    {
        [Obfuscation(Feature = "renaming", Exclude = true)]
        public double Temperature { get; set; }
        [Obfuscation(Feature = "renaming", Exclude = true)]
        public double Setpoint { get; set; }
        [Obfuscation(Feature = "renaming", Exclude = true)]
        public DateTime Timestamp { get; set; }


        public static List<TemperatureData> FromCsv(string csvFilePath)
        {
            var temperatureDataList = new List<TemperatureData>();
            var lines = File.ReadAllLines(csvFilePath).Skip(1);
            foreach (var line in lines)
            {
                try
                {
                    var values = line.Split(',');
                    var temperatureData = new TemperatureData
                    {
                        Timestamp = DateTime.Parse(values[0]),
                        Temperature = double.Parse(values[1]),
                        Setpoint = double.Parse(values[2])
                    };

                    temperatureDataList.Add(temperatureData);
                }
                catch { }
            }
            return temperatureDataList;
        }



        public static List<TemperatureData> SortByTimestamp(List<TemperatureData> temperatureDataList, bool ascending = true)
        {
            return ascending
                ? temperatureDataList.OrderBy(data => data.Timestamp).ToList()
                : temperatureDataList.OrderByDescending(data => data.Timestamp).ToList();
        }
    }
}
