
using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LisaCore.Nlp.BERT
{
    internal class Trainer
    {
        private readonly MLContext _mlContext;

        public Trainer()
        {
            _mlContext = new MLContext(11);
        }

        public ITransformer BuidAndTrain(bool useGpu)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream stream = assembly.GetManifestResourceStream("LisaCore.lib.models.bert.bertsquad.onnx");
            var bertModelPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".onnx");
            // Write the stream to the temporary file
            using (var fileStream = File.Create(bertModelPath))
            {
                stream.CopyTo(fileStream);
            }
            var result = BuidAndTrain(bertModelPath, useGpu);
            //File.Delete(bertModelPath);
            return result;
        }
    

    public ITransformer BuidAndTrain(string bertModelPath, bool useGpu)
        {
            var pipeline = _mlContext.Transforms
                            .ApplyOnnxModel(modelFile: bertModelPath,
                                            outputColumnNames: new[] { "unstack:1",
                                                                       "unstack:0",
                                                                       "unique_ids:0" },
                                            inputColumnNames: new[] {"unique_ids_raw_output___9:0",
                                                                      "segment_ids:0",
                                                                      "input_mask:0",
                                                                      "input_ids:0" },
                                            gpuDeviceId: useGpu ? 0 : (int?)null);

            return pipeline.Fit(_mlContext.Data.LoadFromEnumerable(new List<BertInput>()));
        }
    }
}

