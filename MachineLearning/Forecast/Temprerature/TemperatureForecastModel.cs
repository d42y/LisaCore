using LisaCore.MachineLearning.DataModels;
using MachineLearningLibrary.DataModels;
using Microsoft.ML;
using Microsoft.ML.Transforms.TimeSeries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineLearningLibrary.Forecast.Temprerature
{
    public static class TemperatureForecastModel
    {
        // Define the function to train and save the time-series forecasting model
        //public static void Train(TemperatureData[] historicalData, string modelFilePath)
        //{
        //    // Load the historical data into an IDataView
        //    var mlContext = new MLContext();
        //    var data = mlContext.Data.LoadFromEnumerable(historicalData);

        //    // Define the input and output columns
        //    var inputColumnName = nameof(TemperatureData.Temperature);
        //    var outputColumnName = "Forecast";

        //    // Define the time-series forecasting pipeline
        //    var forecastingPipeline = mlContext.Forecasting.ForecastBySsa(
        //        outputColumnName: outputColumnName,
        //        inputColumnName: inputColumnName,
        //        windowSize: 24,
        //        seriesLength: historicalData.Length,
        //        trainSize: historicalData.Length - 1,
        //        horizon: 60,
        //        confidenceLevel: 0.95f,
        //        confidenceLowerBoundColumn: "LowerBound",
        //        confidenceUpperBoundColumn: "UpperBound"
        //    );

        //    // Train the model and save it to a file
        //    var model = forecastingPipeline.Fit(data);
        //    using (var stream = new FileStream(modelFilePath, FileMode.Create, FileAccess.Write, FileShare.Write))
        //        mlContext.Model.Save(model, data.Schema, stream);
        //}

        //// Define the function to load the time-series forecasting model from a file and use it to predict the temperature
        //public static TemperatureForecastResult Forecast(DateTime timestamp, double setpoint, double currentTemperature, string modelFilePath)
        //{
        //    // Load the model from a file
        //    var mlContext = new MLContext();
        //    ITransformer model;
        //    using (var stream = new FileStream(modelFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
        //        model = mlContext.Model.Load(stream, out _);

        //    // Filter the historical data to only include data points with the given setpoint
        //    var historicalData = new TemperatureData[] {  }; // replace with your historical data
        //    var filteredData = historicalData.Where(d => d.Setpoint == setpoint).ToArray();

        //    // Concatenate the filtered data with the current temperature and setpoint
        //    var allData = filteredData.Append(new TemperatureData
        //    {
        //        Timestamp = timestamp,
        //        Setpoint = setpoint,
        //        Temperature = currentTemperature
        //    }).ToArray();

        //    // Load the data into an IDataView
        //    var dataView = mlContext.Data.LoadFromEnumerable(allData);

        //    // Create a prediction engine and make a forecast for the next 60 minutes
        //    var engine = model.CreateTimeSeriesEngine<TemperatureData, TemperatureForecastResult>(mlContext);
        //    var forecast = engine.Predict(allData[^1]);

        //    // Return the predicted temperature for the next 60 minutes
        //    return forecast;
        //}

    }
}
