using BrickSchema.Net.Classes.Points;
using LisaCore.Behaviors.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LisaCore.Helpers
{
    public static class PointHistoryFunctions
    {
        public static List<(DateTime, double, PointValueQuality)> ResampleData(List<(DateTime, double, PointValueQuality)> data, TimeSpan interval)
        {
            if (data.Count == 0)
            {
                return new List<(DateTime, double, PointValueQuality)>();
            }
            var resampledData = ResampleData(data, data[0].Item1, data[data.Count - 1].Item1, interval);

            return resampledData;
        }

        public static List<(DateTime, double, PointValueQuality)> ResampleData(List<(DateTime, double, PointValueQuality)> data, DateTime begin, DateTime end, TimeSpan interval)
        {
            if (data.Count == 0)
            {
                return new List<(DateTime, double, PointValueQuality)>();
            }

            data.Sort((a, b) => a.Item1.CompareTo(b.Item1));

            DateTime startTime = begin;
            DateTime endTime = end;

            List<(DateTime, double, PointValueQuality)> resampledData = new List<(DateTime, double, PointValueQuality)>();

            for (DateTime currentTime = startTime; currentTime <= endTime; currentTime += interval)
            {
                var (currentValue, quality) = InterpolateValue(data, currentTime);
                resampledData.Add((currentTime, currentValue, quality));
            }

            return resampledData;
        }

        //private static (double, PointValueQuality) InterpolateValue(List<(DateTime, double, PointValueQuality)> data, DateTime targetTime)
        //{
        //    for (int i = 0; i < data.Count - 1; i++)
        //    {
        //        if (targetTime >= data[i].Item1 && targetTime <= data[i + 1].Item1)
        //        {
        //            double timeDiffRatio = (targetTime - data[i].Item1).TotalSeconds / (data[i + 1].Item1 - data[i].Item1).TotalSeconds;
        //            double valueDiff = data[i + 1].Item2 - data[i].Item2;
        //            return data[i].Item2 + timeDiffRatio * valueDiff;
        //        }
        //    }

        //    return data[data.Count - 1].Item2;
        //}

        private static (double, PointValueQuality) InterpolateValue(List<(DateTime, double, PointValueQuality)> data, DateTime targetTime)
        {
            for (int i = 0; i < data.Count - 1; i++)
            {
                if (targetTime == data[i].Item1)
                {
                    // If the target time matches exactly, return the exact value and its quality
                    return (data[i].Item2, data[i].Item3);
                }
                else if (targetTime > data[i].Item1 && targetTime < data[i + 1].Item1)
                {
                    // If the target time falls between two data points, interpolate
                    double timeDiffRatio = (targetTime - data[i].Item1).TotalSeconds / (data[i + 1].Item1 - data[i].Item1).TotalSeconds;
                    double valueDiff = data[i + 1].Item2 - data[i].Item2;
                    return (data[i].Item2 + timeDiffRatio * valueDiff, PointValueQuality.Interpolated);
                }
            }

            // If target time is beyond the last data point, return the last value and its quality
            if (targetTime == data[data.Count - 1].Item1)
            {
                return (data[data.Count - 1].Item2, data[data.Count - 1].Item3);
            }
            else
            {
                // If the target time does not match any data point, consider the quality as Interpolate (or any other default value)
                return (data[data.Count - 1].Item2, PointValueQuality.Interpolated);
            }
        }

        internal static List<(DateTime, double, PointValueQuality)> BuildHistory(List<PointHistory> histories, DateTime startTime, DateTime endTime, int interval)
        {
            List<(DateTime, double, PointValueQuality)> results = new List<(DateTime, double, PointValueQuality)>();

            foreach (var item in histories)
            {
                DateTime timePointer = item.StartTimestamp;
                for (int i = 0; i < item.Values.Count; i++)
                {
                    timePointer = timePointer.AddSeconds(item.Intervals[i]);
                    if (startTime <= timePointer && timePointer <= endTime)
                    {
                        results.Add((timePointer, item.Values[i], item.Qualities[i]));
                    }
                }
            }
            if (interval > 0)
            {
                results = results.Where(x => x.Item3 != PointValueQuality.Bad).ToList();
                results = PointHistoryFunctions.ResampleData(results, TimeSpan.FromSeconds(interval));
            }

            return results;
        }
    }
}
