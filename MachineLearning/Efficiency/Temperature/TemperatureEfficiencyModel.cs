using MachineLearningLibrary.DataModels;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace MachineLearningLibrary.Efficiency.Temperature
{
    public static class TemperatureEfficiencyModel
    {
        //#region Train

        ///// <summary>
        ///// To train the efficiency of temperature and setpoint based on historical data using ML.NET
        ///// </summary>
        ///// <param name="dataPath">The path to a CSV file containing historical temperature and setpoint data</param>
        ///// <param name="modelPath">the path to a file where the trained ML.NET model will be saved.</param>
        ///// <returns>R-squared. The R-squared value ranges from 0 to 1, with higher values indicating a better fit. An R-squared value of 1 indicates a perfect fit, meaning that the model can explain all of the variation in the target variable. An R-squared value of 0 indicates that the model does not explain any of the variation in the target variable.</returns>
        //public static double Train(string dataPath, string modelPath)
        //{
        //    if (!File.Exists(dataPath))
        //    {
        //        throw new FileNotFoundException($"Error: Input data file not found: {dataPath}");
                
        //    }

        //    return Train(TemperatureDatax.FromCsv(dataPath), modelPath);

            
        //}

        ///// <summary>
        ///// To train the efficiency of temperature and setpoint based on historical data using ML.NET
        ///// </summary>
        ///// <param name="dataList">List of TemperatureData objects as input</param>
        ///// <param name="modelPath">the path to a file where the trained ML.NET model will be saved.</param>
        ///// <returns>R-squared. The R-squared value ranges from 0 to 1, with higher values indicating a better fit. An R-squared value of 1 indicates a perfect fit, meaning that the model can explain all of the variation in the target variable. An R-squared value of 0 indicates that the model does not explain any of the variation in the target variable.</returns>

        //public static double Train(List<TemperatureDatax> dataList, string modelPath)
        //{
        //    if (!Directory.Exists(Path.GetDirectoryName(modelPath)) && !string.IsNullOrEmpty(Path.GetDirectoryName(modelPath)))
        //    {
        //        throw new DirectoryNotFoundException($"Error: Model directory not found: {Path.GetDirectoryName(modelPath)}");

        //    }
        //    // Calculate the required values for each data point in dataList
        //    dataList = TemperatureDatax.AddCalculatedValues(dataList);


        //    var mlContext = new MLContext();
        //    // Load the data
        //    var dataView = mlContext.Data.LoadFromEnumerable(dataList);

        //    return Train(dataView, modelPath);

        //}

        //private static double Train(IDataView dataView, string modelPath)
        //{
        //    var mlContext = new MLContext();
        //    // Define the data pipeline
        //    var pipeline = mlContext.Transforms.CopyColumns("TemperatureDifference", "TemperatureDifference")
        //        .Append(mlContext.Transforms.CopyColumns("TemperatureRateOfChange", "TemperatureRateOfChange"))
        //        .Append(mlContext.Transforms.Concatenate("Features", "TemperatureDifference", "TemperatureRateOfChange"))
        //        .Append(mlContext.Transforms.NormalizeMinMax("Features"))
        //        .Append(mlContext.Regression.Trainers.Sdca(
        //            labelColumnName: "EfficiencyMetric",
        //            featureColumnName: "Features"));

        //    // Perform k-fold cross-validation
        //    var crossValidationResults = mlContext.Regression.CrossValidate(dataView, pipeline, numberOfFolds: 5);

        //    // Find the best model based on R-squared
        //    var bestModelMetrics = crossValidationResults
        //        .OrderByDescending(m => m.Metrics.RSquared)
        //        .FirstOrDefault();

        //    // Use the best model for evaluation and saving
        //    var bestModel = bestModelMetrics.Model;

        //    // Evaluate the model
        //    var predictions = bestModel.Transform(dataView);
        //    var metrics = mlContext.Regression.Evaluate(predictions, labelColumnName: "Temperature");

        //    // Output the results
        //    Console.WriteLine($"Temperature efficiency (R-squared): {metrics.RSquared}");

        //    try
        //    {
        //        // Save the model
        //        mlContext.Model.Save(bestModel, null, modelPath);
        //        return metrics.RSquared;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new FieldAccessException($"Error saving model to file: {ex.Message}");

        //    }

        //}


        //#endregion

        //#region Calculate
       
        //public static List<TemperatureEfficiencyResult> Calculate(List<TemperatureDatax> dataList, string modelFile)
        //{
        //    if (dataList == null || !dataList.Any())
        //    {
        //        throw new ArgumentNullException("Error: Input data list is null or empty.");
                
        //    }

        //    // Calculate the required values for each data point in dataList
        //    dataList = TemperatureDatax.AddCalculatedValues(dataList);

        //    // Load the model
        //    var mlContext = new MLContext();
        //    var model = mlContext.Model.Load(modelFile, out _);

        //    // Create a prediction engine
        //    var predictionEngine = mlContext.Model.CreatePredictionEngine<TemperatureDatax, TemperatureEfficiencyResult>(model);

        //    // Create a list to hold the efficiency results
        //    var efficiencyList = new List<TemperatureEfficiencyResult>();

        //    // Use the prediction engine to calculate the efficiency for each input data point
        //    foreach (var dataPoint in dataList)
        //    {
        //        var prediction = predictionEngine.Predict(dataPoint);
        //        efficiencyList.Add(prediction);
        //    }

        //    // Return the efficiency results
        //    return efficiencyList;
        //}


        //#endregion

        //private static void CalculateTemperatureRateOfChange(List<TemperatureDatax> dataList)
        //{
        //    if (dataList.Count < 2)
        //    {
        //        return;
        //    }

        //    // Iterate through the dataList and calculate the rate of change based on the previous data point's temperature
        //    for (int i = 1; i < dataList.Count; i++)
        //    {
        //        double previousTemperature = dataList[i - 1].Temperature;
        //        double currentTemperature = dataList[i].Temperature;
        //        double timeDifference = (dataList[i].Timestamp - dataList[i - 1].Timestamp).TotalSeconds;

        //        if (timeDifference > 0)
        //        {
        //            dataList[i].TemperatureRateOfChange = (currentTemperature - previousTemperature) / timeDifference;
        //        }
        //        else
        //        {
        //            dataList[i].TemperatureRateOfChange = 0;
        //        }
        //    }

        //    // Set the rate of change for the first data point to 0 or any other suitable default value
        //    dataList[0].TemperatureRateOfChange = 0;
        //}
    }
}
