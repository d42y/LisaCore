using BrickSchema.Net.Classes.Points;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LisaCore.Helpers
{
    internal class RunLengthEncodingConverter<T> : ValueConverter<List<T>, string>
    {
        public RunLengthEncodingConverter() : base(
            v => Compress(v),
            v => Decompress(v))
        {
        }

        private static string Compress(List<T> values)
        {
            //var compressed = new List<Tuple<T, int>>();
            var compressed = new List<string>();
            T lastValue = values[0];
            int count = 1;




            for (int i = 1; i < values.Count; i++)
            {

                if (values[i].Equals(lastValue))
                {
                    count++;
                }
                else
                {
                    compressed.Add($"{ConvertToString(lastValue)}:{count}");
                    lastValue = values[i];
                    count = 1;
                }

            }

            compressed.Add($"{ConvertToString(lastValue)}:{count}");
            string text = System.Text.Json.JsonSerializer.Serialize(compressed);
            return text;
        }

        private static string ConvertToString(T value)
        {
            string val = string.Empty;
            if (typeof(T) == typeof(PointValueQuality))
            {
                var status = (PointValueQuality)Convert.ChangeType(value, typeof(PointValueQuality), CultureInfo.InvariantCulture);
                val = status.ToString();
            }
            else if (typeof(T) == typeof(double) || typeof(T) == typeof(Double))
            {
                var num = (double)Convert.ChangeType(value, typeof(double), CultureInfo.InvariantCulture);
                val = num.ToString();
            }
            else
            {
                val = System.Text.Json.JsonSerializer.Serialize(value);
            }

            return val;
        }

        private static List<T> Decompress(string data)
        {
            //var values = System.Text.Json.JsonSerializer.Deserialize<List<Tuple<T, int>>>(data);

            List<string> values = new();
            try
            {
                values = System.Text.Json.JsonSerializer.Deserialize<List<string>>(data);
            }
            catch
            {
                values = new();
            }

            var decompressed = new List<T>();
            foreach (var val in values ?? new())
            {
                T? value = default;
                var index = val.LastIndexOf(":");
                var v = val.Substring(0, index);
                var num = val.Substring(index + 1);
                int count = int.Parse(num);
                if (typeof(T) == typeof(PointValueQuality))
                {
                    var status = Enum.Parse<PointValueQuality>(v);
                    value = (T)Convert.ChangeType(status, typeof(T), CultureInfo.InvariantCulture);
                }
                else if (typeof(T) == typeof(double) || typeof(T) == typeof(Double))
                {
                    var number = double.Parse(v);
                    value = (T)Convert.ChangeType(number, typeof(T), CultureInfo.InvariantCulture);
                }
                else
                {
                    value = System.Text.Json.JsonSerializer.Deserialize<T>(v);
                }
                decompressed.AddRange(Enumerable.Repeat(value, count));
            }
            return decompressed;
        }
    }
}
