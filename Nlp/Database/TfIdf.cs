using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LisaCore.Nlp.Database
{
    public class TfIdf
    {
        private readonly Dictionary<string, string> _documents;
        private Dictionary<string, Dictionary<string, double>> _tfidf;

        public TfIdf()
        {
            _documents = new Dictionary<string, string>();
            _tfidf = new Dictionary<string, Dictionary<string, double>>();
        }
        public TfIdf(Dictionary<string, string> documents)
        {
            _documents = documents.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToLower());
            _tfidf = CalculateTfIdf();
        }

        public void AddDocument(string key, string document)
        {
            _documents[key] = document.ToLower();
            _tfidf = CalculateTfIdf(); // recalculate after adding new document
        }

        public Dictionary<string, Dictionary<string, double>> CalculateTfIdf()
        {
            var tf = CalculateTermFrequency();
            var idf = CalculateInverseDocumentFrequency();

            foreach (var document in tf)
            {
                foreach (var term in document.Value)
                {
                    document.Value[term.Key] *= idf[term.Key];
                }
            }

            return tf;
        }

        private Dictionary<string, Dictionary<string, double>> CalculateTermFrequency()
        {
            var tf = new Dictionary<string, Dictionary<string, double>>();

            foreach (var document in _documents)
            {
                var wordCounts = new Dictionary<string, double>();
                var totalWordCount = 0.0;

                var words = Tokenize(document.Value);

                foreach (var word in words)
                {
                    totalWordCount++;

                    if (wordCounts.ContainsKey(word))
                    {
                        wordCounts[word]++;
                    }
                    else
                    {
                        wordCounts[word] = 1;
                    }
                }

                foreach (var word in wordCounts.Keys.ToList())
                {
                    wordCounts[word] /= totalWordCount;
                }

                tf[document.Key] = wordCounts;
            }

            return tf;
        }

        private Dictionary<string, double> CalculateInverseDocumentFrequency()
        {
            var idf = new Dictionary<string, double>();

            var allWords = new HashSet<string>();

            foreach (var document in _documents.Values)
            {
                var words = Tokenize(document);

                foreach (var word in words)
                {
                    allWords.Add(word);
                }
            }

            foreach (var word in allWords)
            {
                var count = _documents.Values.Count(d => d.Contains(word));
                idf[word] = Math.Log(_documents.Count / (double)count);
            }

            return idf;
        }

        private string[] Tokenize(string document)
        {
            return Regex.Matches(document, @"\b\w+\b").OfType<Match>().Select(m => m.Value).ToArray();
        }

        public double CalculateCosineSimilarity(Dictionary<string, double> vec1, Dictionary<string, double> vec2)
        {
            var dotProduct = 0.0;
            var magnitude1 = 0.0;
            var magnitude2 = 0.0;

            foreach (var pair in vec1)
            {
                dotProduct += pair.Value * vec2.GetValueOrDefault(pair.Key, 0);
                magnitude1 += pair.Value * pair.Value;
            }

            foreach (var val in vec2.Values)
            {
                magnitude2 += val * val;
            }

            var magnitude = Math.Sqrt(magnitude1) * Math.Sqrt(magnitude2);

            return magnitude == 0 ? 0 : dotProduct / magnitude;
        }

        public Dictionary<string, double> Search(string searchTerm)
        {
            var scores = new Dictionary<string, double>();
            var searchVector = new Dictionary<string, double>();
            var searchTerms = Tokenize(searchTerm.ToLower());

            foreach (var term in searchTerms)
            {
                if (!searchVector.ContainsKey(term))
                {
                    searchVector[term] = 1;
                }
            }

            foreach (var document in _tfidf)
            {
                var score = CalculateCosineSimilarity(searchVector, document.Value);
                scores[document.Key] = score;
            }

            return scores;
        }

        public string GetValue(string key) { 
            if (_documents.ContainsKey(key)) return _documents[key];

            return string.Empty;
        }

    }
}
