using System;
using System.IO;
using Microsoft.ML;
using ml_model.DataModels;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.ML.Trainers;

namespace ml_model
{
    public class Program
    {
        private string _ModelSavePath;
        private string _TrainDataPath;
        private string _TestDataPath;



        static void Main(string[] args)
        {
            Program prog = new Program();
            prog.dostuff(args);
            Console.ReadLine();
        }

        private void dostuff(string[] args)
        {
            Console.WriteLine("With this program you can train and elvaluate ML Models, as well as use them.");
            while (true)
            {
                Console.WriteLine("Press 1 to build a new model.");
                Console.WriteLine("Press 2 to evaluate a model");
                Console.WriteLine("Press 3 to use a model on a file.");
                MLContext mLContext = new MLContext(seed: 1);
                var selection = Console.ReadLine();
                switch(selection)
                {
                    case "1":
                        Console.WriteLine("Training data:");
                        _TrainDataPath = Console.ReadLine();
                        Console.WriteLine("Test data:");
                        _TestDataPath = Console.ReadLine();
                        Console.WriteLine("Place to save the model to:");
                        _ModelSavePath = Console.ReadLine();
                        var model = Train(mLContext, _TrainDataPath);
                        mLContext.Model.Save(model, null, _ModelSavePath);
                        Evaluate(mLContext, _ModelSavePath);
                        Console.WriteLine("Done");
                        Console.ReadLine();
                        break;
                    case "2":
                        Console.WriteLine("Model to be evaluated:");
                        string modelPath = Console.ReadLine();
                        Console.WriteLine("Test data:");
                        _TestDataPath = Console.ReadLine();
                        Evaluate(mLContext, modelPath);
                        Console.WriteLine("Done");
                        Console.ReadLine();
                        break;
                    case "3":
                        Console.WriteLine("Model to be used:");
                        var modelPath1 = Console.ReadLine();
                        Console.WriteLine("Data to predict:");
                        var loadPath = Console.ReadLine();
                        Console.WriteLine("Place to save the prediction:");
                        var savePath = Console.ReadLine();
                        PredictWholeFile(mLContext, modelPath1, loadPath, savePath);
                        Console.WriteLine("Done");
                        Console.ReadLine();
                        break;
                    default:
                        Console.WriteLine("invalid input");
                        break;
                }
            }
        }

        public ITransformer Train(MLContext mLContext, string dataPath)
        {
            IDataView traingDataView = mLContext.Data.LoadFromTextFile<ModelInput>(path: dataPath, hasHeader: true, separatorChar: ',');
            Console.WriteLine("Loaded data");
            var dataProcessPipeline = mLContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: "Prediction")
                .Append(mLContext.Transforms.Concatenate("Features", new[] { "TempNewObs", "1_Temp", "1_DistToLast", "1_DistToAllTimeMean", "1_DistToTempMean", "1_DistToRef", "2_Temp", "2_DistToLast", "2_DistToAllTimeMean", "2_DistToTempMean", "2_DistToRef", "3_Temp", "3_DistToLast", "3_DistToAllTimeMean", "3_DistToTempMean", "3_DistToRef", "4_Temp", "4_DistToLast", "4_DistToAllTimeMean", "4_DistToTempMean", "4_DistToRef", "5_Temp", "5_DistToLast", "5_DistToAllTimeMean", "5_DistToTempMean", "5_DistToRef" })
                .Append(mLContext.Transforms.NormalizeMinMax("Features"))
                .Append(mLContext.Regression.Trainers.Sdca()));
            Console.WriteLine("Builded pipeline");
            Console.WriteLine("Starting training ...");
            var model = dataProcessPipeline.Fit(traingDataView);
            Console.WriteLine("finished training");
            return model;
        }

        public void Evaluate( MLContext mLContext, string modelPath)
        {
            IDataView TestDataView = mLContext.Data.LoadFromTextFile<ModelInput>(path: _TestDataPath, hasHeader: true, separatorChar: ',');
            ITransformer model = mLContext.Model.Load(modelPath, out DataViewSchema inputSchema);
            var predictions = model.Transform(TestDataView);
            var metrics = mLContext.Regression.Evaluate(predictions, "Label", "Score");
            Console.WriteLine($"*************************************************");
            Console.WriteLine($"*       Model quality metrics evaluation         ");
            Console.WriteLine($"*------------------------------------------------");
            Console.WriteLine($"*       RSquared Score:      {metrics.RSquared:0.##}");
            Console.WriteLine($"*       Root Mean Squared Error:      {metrics.RootMeanSquaredError:#.##}");
        }

        private void PredictWholeFile(MLContext mLContext, string modelPath, string dataFilePath, string savePath)
        {
            ITransformer mlModel = mLContext.Model.Load(modelPath, out DataViewSchema inputSchema);
            var predEngine = mLContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(mlModel);
            var data = File.ReadAllLines(dataFilePath);
            List<string> list = new List<string>();
            string[] result = new string[3];
            foreach (var line in data)
            {
                string[] split = line.Split(',');
                ModelInput sampleData = new ModelInput()
                {
                    Prediction = (float)Convert.ToDouble(split[0], CultureInfo.InvariantCulture),
                    TempNewObs = (float)Convert.ToDouble(split[1], CultureInfo.InvariantCulture),
                    Col1_Temp = (float)Convert.ToDouble(split[2], CultureInfo.InvariantCulture),
                    Col1_DistToLast = (float)Convert.ToDouble(split[3], CultureInfo.InvariantCulture),
                    Col1_DistToAllTimeMean = (float)Convert.ToDouble(split[4], CultureInfo.InvariantCulture),
                    Col1_DistToTempMean = (float)Convert.ToDouble(split[5], CultureInfo.InvariantCulture),
                    Col1_DistToRef = (float)Convert.ToDouble(split[6], CultureInfo.InvariantCulture),
                    Col2_Temp = (float)Convert.ToDouble(split[7], CultureInfo.InvariantCulture),
                    Col2_DistToLast = (float)Convert.ToDouble(split[8], CultureInfo.InvariantCulture),
                    Col2_DistToAllTimeMean = (float)Convert.ToDouble(split[9], CultureInfo.InvariantCulture),
                    Col2_DistToTempMean = (float)Convert.ToDouble(split[10], CultureInfo.InvariantCulture),
                    Col2_DistToRef = (float)Convert.ToDouble(split[11], CultureInfo.InvariantCulture),
                    Col3_Temp = (float)Convert.ToDouble(split[12], CultureInfo.InvariantCulture),
                    Col3_DistToLast = (float)Convert.ToDouble(split[13], CultureInfo.InvariantCulture),
                    Col3_DistToAllTimeMean = (float)Convert.ToDouble(split[14], CultureInfo.InvariantCulture),
                    Col3_DistToTempMean = (float)Convert.ToDouble(split[15], CultureInfo.InvariantCulture),
                    Col3_DistToRef = (float)Convert.ToDouble(split[16], CultureInfo.InvariantCulture),
                    Col4_Temp = (float)Convert.ToDouble(split[17], CultureInfo.InvariantCulture),
                    Col4_DistToLast = (float)Convert.ToDouble(split[18], CultureInfo.InvariantCulture),
                    Col4_DistToAllTimeMean = (float)Convert.ToDouble(split[19], CultureInfo.InvariantCulture),
                    Col4_DistToTempMean = (float)Convert.ToDouble(split[20], CultureInfo.InvariantCulture),
                    Col4_DistToRef = (float)Convert.ToDouble(split[21], CultureInfo.InvariantCulture),
                    Col5_Temp = (float)Convert.ToDouble(split[22], CultureInfo.InvariantCulture),
                    Col5_DistToLast = (float)Convert.ToDouble(split[23], CultureInfo.InvariantCulture),
                    Col5_DistToAllTimeMean = (float)Convert.ToDouble(split[24], CultureInfo.InvariantCulture),
                    Col5_DistToTempMean = (float)Convert.ToDouble(split[25], CultureInfo.InvariantCulture),
                    Col5_DistToRef = (float)Convert.ToDouble(split[26], CultureInfo.InvariantCulture),
                };
                ModelOutput modelOutput = predEngine.Predict(sampleData);
                result = new string[3] { modelOutput.Score.ToString(CultureInfo.InvariantCulture), sampleData.Prediction.ToString(CultureInfo.InvariantCulture), (sampleData.Prediction - modelOutput.Score).ToString(CultureInfo.InvariantCulture) };
                list.Add(string.Join(",", result));
            }
            string[] namesplit = dataFilePath.Split('\\');
            File.WriteAllLines(savePath + "prediction" + "_" + namesplit[namesplit.Length-1], list);
        }
    }
}
