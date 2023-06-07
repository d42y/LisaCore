using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineLearningLibrary.Functions
{
    internal static class Helper
    {
        internal static TimeSpan GetInterval(List<DateTime> timestamps)
        {
            // Compute the average interval between consecutive timestamps
            double totalTicks = 0;
            for (int i = 1; i < timestamps.Count; i++)
            {
                totalTicks += (timestamps[i] - timestamps[i - 1]).Ticks;
            }
            double avgTicks = totalTicks / (timestamps.Count - 1);
            TimeSpan interval = TimeSpan.FromTicks((long)avgTicks);

            return interval;
        }


        //public static List<T> LoadData<T>(string filePath)
        //{
        //    var mlContext = new MLContext();
        //    var dataView = mlContext.Data.LoadFromTextFile<T>(filePath, separatorChar: ',');
        //    List<T> dataList = IDataViewToList<T>(mlContext, dataView);
        //    return dataList;
        //}

        //public static List<T> IDataViewToList<T>(MLContext mlContext, IDataView dataView) where T : class, new()
        //{
        //    var list = new List<T>();

        //    var cursor = dataView.GetRowCursor(dataView.Schema);
        //    var getter = cursor.GetGetter<T>();

        //    while (cursor.MoveNext())
        //    {
        //        T item = new T();
        //        getter(ref item);
        //        list.Add(item);
        //    }

        //    return list;
        //}
    }
}
