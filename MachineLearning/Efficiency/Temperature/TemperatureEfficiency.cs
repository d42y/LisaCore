using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.ML;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.ML.DataOperationsCatalog;

namespace LisaCore.MachineLearning.Efficiency.Temperature
{
    public class TemperatureEfficiency
    {
        private readonly MLContext mlContext;
        private ITransformer _model;
        private ILogger? _logger;
        public TemperatureEfficiency (ILogger? logger = null)
        {
            mlContext = new MLContext();
            _logger = logger;
        }

        public List<TemperatureEfficiencyResult> CalculateEfficiency (List<TemperatureData> tempData, double deadband)
        {
            _logger?.LogInformation("Enter Calculate Efficiency");
            _logger?.LogInformation(JsonConvert.SerializeObject(tempData, Formatting.Indented));
            _logger?.LogInformation($"Deadband: {deadband}");
            var sortedData = TemperatureData.SortByTimestamp(tempData);
            var formattedData = TemperatureDataCalculated.FormatTemperatureData(sortedData);

            _logger?.LogInformation(JsonConvert.SerializeObject(formattedData, Formatting.Indented));
            var efficencyData = CalculateEfficiency(formattedData, deadband);
            return efficencyData;
        }


        /*
         * ASHRAE 55
         * https://en.wikipedia.org/wiki/ASHRAE_55
         * Temperature variations with time When occupants do not have control over the cyclical variation or drifts in indoor environmental conditions, the conditions within this section must be met. 
           Operative temperatures may not fluctuate more than 1.1 °C (2.0 °F) within 15 minutes, nor change more than 2.2 °C (4.0 °F) within 1 hour.[1]. 
         */


        public static List<TemperatureEfficiencyResult> CalculateEfficiency(List<TemperatureDataCalculated> temperatureDataList, double deadband)
        {

            List<TemperatureEfficiencyResult> efficiencyContributions = new List<TemperatureEfficiencyResult>();

            for (int i = 1; i < temperatureDataList.Count; i++)
            {
                TemperatureData current = temperatureDataList[i];
                
                temperatureDataList[i].DistanceFromSetpoint = Math.Abs(current.Temperature - current.Setpoint);
                double deviationScore = 100 - (temperatureDataList[i].DistanceFromSetpoint / (deadband / 2)) * 100;

                double violationScore = 100;
                for (int j = i - 1; j >= 0; j--)
                {
                    TemperatureData previous = temperatureDataList[j];
                    TimeSpan timeDifference = current.Timestamp - previous.Timestamp;

                    double temperatureDifference = Math.Abs(current.Temperature - previous.Temperature);

                    if (timeDifference <= TimeSpan.FromMinutes(15) && temperatureDifference > (deadband / 2))
                    {
                        violationScore = 0;
                        break;
                    }

                    if (timeDifference <= TimeSpan.FromHours(1) && temperatureDifference > deadband)
                    {
                        violationScore = 0;
                        break;
                    }

                    if (timeDifference > TimeSpan.FromHours(1))
                    {
                        break;
                    }
                }

                var result = TemperatureEfficiencyResult.Convert(temperatureDataList[i]);
                result.DeviationScore = (float)deviationScore;
                result.ViolationScore = (float)violationScore;
                double efficiencyRating = (deviationScore + violationScore) / 2;
                result.Efficiency = (float)Math.Max(0, efficiencyRating);

                efficiencyContributions.Add(result);
            }

            return efficiencyContributions;
        }

        //public void TrainModel(string dataPath)
        //{
        //    var data = LoadAndPreprocessData(dataPath, 2);
        //    var tt = GetTrainTestData(data);

        //    var pipeline = mlContext.Transforms.CopyColumns("DistanceFromSetpoint", "DistanceFromSetpoint")
        //        .Append(mlContext.Transforms.CopyColumns("TemperatureChange", "TemperatureChange"))
        //        .Append(mlContext.Transforms.Concatenate("Features", "DistanceFromSetpoint", "TemperatureChange"))
        //        .Append(mlContext.Transforms.NormalizeMinMax("Features"))
        //        .Append(mlContext.Regression.Trainers.Sdca(
        //            labelColumnName: "Efficiency",
        //            featureColumnName: "Features"));

        //    _model = pipeline.Fit(tt.TrainSet);
        //}

        //public TrainTestData GetTrainTestData(IDataView data)
        //{
        //    return mlContext.Data.TrainTestSplit(data);
        //}

        //public IDataView LoadAndPreprocessData(string dataPath, float rateOfChange, float deadband)
        //{
        //    var tempData = TemperatureData.FromCsv(dataPath);
        //    tempData = TemperatureData.SortByTimestamp(tempData);
        //    tempData = TemperatureData.SmoothTemperatureData(tempData);
        //    var trainingData = tempData;
        //    //var rawData = mlContext.Data.LoadFromTextFile<TemperatureData>(dataPath, separatorChar: ',', hasHeader: true);
        //    var rawData = mlContext.Data.LoadFromEnumerable(trainingData);
        //    var enumerableData = mlContext.Data.CreateEnumerable<TemperatureTrainingData>(rawData, reuseRowObject: false);
        //    var preprocessedData = PreprocessData(enumerableData, deadband);
        //    return mlContext.Data.LoadFromEnumerable(preprocessedData);
        //}


        //public void EvaluateModel(DataViewSchema schema, IDataView testData)
        //{
        //    var predictions = _model.Transform(testData);
        //    var metrics = mlContext.Regression.Evaluate(predictions);
        //    Console.WriteLine($"R-squared: {metrics.RSquared:0.###}");
        //    Console.WriteLine($"MSE: {metrics.MeanSquaredError:#.###}");
        //}

        //public void SaveModel(string modelPath, DataViewSchema schema)
        //{
        //    mlContext.Model.Save(_model, schema, modelPath);
        //}

        //public float PredictEfficiency(TemperatureData sampleData)
        //{
        //    var predictionEngine = mlContext.Model.CreatePredictionEngine<TemperatureData, TemperatureEfficiencyResult>(_model);
        //    var prediction = predictionEngine.Predict(sampleData);
        //    return prediction.Efficiency;
        //}

        //public static List<TemperatureTrainingData> PreprocessData(IEnumerable<TemperatureTrainingData> rawData, float deadband)
        //{
        //    var preprocessedData = new List<TemperatureTrainingData>();
        //    TemperatureTrainingData prevData = null;
        //    foreach (var data in rawData)
        //    {

        //        if (prevData != null)
        //        {
        //            double timeDifference = (data.Timestamp - prevData.Timestamp).TotalSeconds;
        //            data.TemperatureChangeRate = (data.Temperature - prevData.Temperature) / (float)timeDifference;
        //        }
        //        else
        //        {
        //            data.TemperatureChangeRate = 0;

        //        }
        //        data.DistanceFromSetpoint = Math.Abs(data.Temperature - data.Setpoint);

        //        preprocessedData.Add(data);
        //        prevData = data;

        //    }

        //    return preprocessedData;
        //}

    }
}
