using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineLearningLibrary.DataModels
{
    public class TemperatureForecastResult
    {
        [VectorType(1)]
        public double[] Forecast { get; set; }

        [VectorType(1)]
        public double[] LowerBound { get; set; }

        [VectorType(1)]
        public double[] UpperBound { get; set; }
        [VectorType(1)]
        public double[] ConfidenceLowerBound { get; set; }

        [VectorType(1)]
        public double[] ConfidenceUpperBound { get; set; }
    }
}
