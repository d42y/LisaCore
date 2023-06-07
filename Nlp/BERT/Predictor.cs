using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LisaCore.Nlp.BERT
{
    internal class Predictor
    {
        private MLContext _mLContext;
        private PredictionEngine<BertInput, BertPredictions> _predictionEngine;

        public Predictor(ITransformer trainedModel)
        {
            _mLContext = new MLContext();
            _predictionEngine = _mLContext.Model
                                          .CreatePredictionEngine<BertInput, BertPredictions>(trainedModel);
        }

        public BertPredictions Predict(BertInput encodedInput)
        {
            return _predictionEngine.Predict(encodedInput);
        }
    }

}
