using LisaCore.Extensions;
using LisaCore.Helpers;
using LisaCore.Tokenizers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LisaCore.Nlp.BERT
{
    internal class Bert
    {

        private Dictionary<int, string> _context { get; set; } = new();
        private List<string> _vocabulary;

        private readonly Tokenizer _tokenizer;
        private Predictor _predictor;

        public Bert(string vocabularyFilePath, string bertModelPath, string? contextFilePath = null, bool useGpu = false)
        {
            _vocabulary = FileReader.LoadVocabFromFile(vocabularyFilePath);
            _tokenizer = new Tokenizer(_vocabulary);

            var trainer = new Trainer();
            var trainedModel = trainer.BuidAndTrain(bertModelPath, useGpu);
            _predictor = new Predictor(trainedModel);
            LoadContextFromFile(contextFilePath);
        }

        public void SetContext(Dictionary<int, string> context)
        {
            _context.Clear();
            _context = context;
        }
        public void LoadContextFromFile(string contextFilePath)
        {
            if (!string.IsNullOrEmpty(contextFilePath) && File.Exists(contextFilePath))
            {
                _context = FileReader.LoadContextFromFile(contextFilePath);
            }
        }
        public (List<string> tokens, float probability) Predict(string question)
        {
            var keywords = question.Split(' ')
                       .Where(keyword => !StopWords.Words.Contains(keyword.ToLower()))
                       .ToList();
            var bestMatch = _context.OrderByDescending(section => keywords.Count(keyword => section.Value.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                            .FirstOrDefault();
            return Predict(bestMatch.Value, question);
        }

        public (List<string> tokens, float probability) Predict(Dictionary<int, string> context, string question)
        {
            var keywords = question.Split(' ')
                       .Where(keyword => !StopWords.Words.Contains(keyword.ToLower()))
                       .ToList();
            var bestMatch = context.OrderByDescending(section => keywords.Count(keyword => section.Value.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                            .FirstOrDefault();
            return Predict(bestMatch.Value, question);
        }

        public (List<string> tokens, float probability) Predict(string context, string question)
        {
            var tokens = _tokenizer.Tokenize(question, context);
            var input = BuildInput(tokens);

            var predictions = _predictor.Predict(input);

            var contextStart = tokens.FindIndex(o => o.Token == Tokens.Separation);

            var (startIndex, endIndex, probability) = GetBestPrediction(predictions, contextStart, 20, 30);

            var predictedTokens = input.InputIds
                .Skip(startIndex)
                .Take(endIndex + 1 - startIndex)
                .Select(o => _vocabulary[(int)o])
                .ToList();

            var connectedTokens = _tokenizer.Untokenize(predictedTokens);

            return (connectedTokens, probability);
        }

        private BertInput BuildInput(List<(string Token, int Index, long SegmentIndex)> tokens)
        {
            //var padding = Enumerable.Repeat(0L, 256 - tokens.Count).ToList();
            var padding = Enumerable.Repeat(0L, 256 - tokens.Count).ToList();
            var tokenIndexes = tokens.Select(token => (long)token.Index).Concat(padding).ToArray();
            var segmentIndexes = tokens.Select(token => token.SegmentIndex).Concat(padding).ToArray();
            var inputMask = tokens.Select(o => 1L).Concat(padding).ToArray();

            return new BertInput()
            {
                InputIds = tokenIndexes,
                SegmentIds = segmentIndexes,
                InputMask = inputMask,
                UniqueIds = new long[] { 0 }
            };
        }

        private (int StartIndex, int EndIndex, float Probability) GetBestPrediction(BertPredictions result, int minIndex, int topN, int maxLength)
        {
            var bestStartLogits = result.StartLogits
                .Select((logit, index) => (Logit: logit, Index: index))
                .OrderByDescending(o => o.Logit)
                .Take(topN);

            var bestEndLogits = result.EndLogits
                .Select((logit, index) => (Logit: logit, Index: index))
                .OrderByDescending(o => o.Logit)
                .Take(topN);

            var bestResultsWithScore = bestStartLogits
                .SelectMany(startLogit =>
                    bestEndLogits
                    .Select(endLogit =>
                        (
                            StartLogit: startLogit.Index,
                            EndLogit: endLogit.Index,
                            Score: startLogit.Logit + endLogit.Logit
                        )
                     )
                )
                .Where(entry => !(entry.EndLogit < entry.StartLogit || entry.EndLogit - entry.StartLogit > maxLength || entry.StartLogit == 0 && entry.EndLogit == 0 || entry.StartLogit < minIndex))
                .Take(topN);

            var (item, probability) = bestResultsWithScore
                .Softmax(o => o.Score)
                .OrderByDescending(o => o.Probability)
                .FirstOrDefault();

            return (StartIndex: item.StartLogit, EndIndex: item.EndLogit, probability);
        }
    }
}
