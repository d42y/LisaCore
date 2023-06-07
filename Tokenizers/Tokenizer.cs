using LisaCore.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnidecodeSharpCore;

namespace LisaCore.Tokenizers
{
    internal class Tokenizer
    {
        private readonly List<string> _vocabulary;

        public Tokenizer(List<string> vocabulary)
        {
            _vocabulary = vocabulary;
        }

        public List<(string Token, int VocabularyIndex, long SegmentIndex)> Tokenize(params string[] texts)
        {
            IEnumerable<string> tokens = new string[] { Tokens.Classification };

            foreach (var text in texts)
            {
                tokens = tokens.Concat(TokenizeSentence(text));
                tokens = tokens.Concat(new string[] { Tokens.Separation });
            }


         
                var tokenAndIndex = tokens
                    .SelectMany(TokenizeSubwords)
                    .ToList();

                var segmentIndexes = SegmentIndex(tokenAndIndex);

                return tokenAndIndex.Zip(segmentIndexes, (tokenindex, segmentindex)
                                    => (tokenindex.Token, tokenindex.VocabularyIndex, segmentindex)).ToList();

        }

        //public List<(string Token, int VocabularyIndex, long SegmentIndex)> Tokenize(params string[] texts)
        //{
        //    var cancellationTokenSource = new CancellationTokenSource();
        //    var timeout = TimeSpan.FromSeconds(30); // Set the desired timeout period here
        //    cancellationTokenSource.CancelAfter(timeout);


        //        IEnumerable<string> tokens = new string[] { Tokens.Classification };

        //        foreach (var text in texts)
        //        {
        //            tokens = tokens.Concat(TokenizeSentence(text));
        //            tokens = tokens.Concat(new string[] { Tokens.Separation });
        //        }

        //    //var tokenAndIndex = tokens
        //    //    .AsParallel()
        //    //    .WithCancellation(cancellationTokenSource.Token)
        //    //    .SelectMany(TokenizeSubwords)
        //    //    .ToList();
        //    char[] delimiters = new char[] { ' ', '\"'};
        //    var tokenAndIndex = new List<(string Token, int VocabularyIndex)>();
        //        foreach (var token in tokens)
        //        {
        //        //if (cancellationTokenSource.Token.IsCancellationRequested)
        //        //{
        //        //    throw new OperationCanceledException();
        //        //}
        //        var cleanToken = token.Trim(delimiters);
        //            var subwordTokens = TokenizeSubwords(cleanToken);
        //            tokenAndIndex.AddRange(subwordTokens);
        //        }


        //        var segmentIndexes = SegmentIndex(tokenAndIndex);

        //        return tokenAndIndex.Zip(segmentIndexes, (tokenindex, segmentindex)
        //                                => (tokenindex.Token, tokenindex.VocabularyIndex, segmentindex)).ToList();


        //}


        public List<string> Untokenize(List<string> tokens)
        {
            var currentToken = string.Empty;
            var untokens = new List<string>();
            tokens.Reverse();

            tokens.ForEach(token =>
            {
                if (token.StartsWith("##"))
                {
                    currentToken = token.Replace("##", "") + currentToken;
                }
                else
                {
                    currentToken = token + currentToken;
                    untokens.Add(currentToken);
                    currentToken = string.Empty;
                }
            });

            untokens.Reverse();

            return untokens;
        }

        public IEnumerable<long> SegmentIndex(List<(string token, int index)> tokens)
        {
            var segmentIndex = 0;
            var segmentIndexes = new List<long>();

            foreach (var (token, index) in tokens)
            {
                segmentIndexes.Add(segmentIndex);

                if (token == Tokens.Separation)
                {
                    segmentIndex++;
                }
            }

            return segmentIndexes;
        }

        private IEnumerable<(string Token, int VocabularyIndex)> TokenizeSubwords(string word)
        {
            //word = word.Trim('\n').Trim('\r'); //these two char causes the loop to go infinate
            if (_vocabulary.Contains(word))
            {
                return new (string, int)[] { (word, _vocabulary.IndexOf(word)) };
            }

            var tokens = new List<(string, int)>();
            var remaining = word;

            while (!string.IsNullOrEmpty(remaining) && remaining.Length > 2)
            {
                var prefix = _vocabulary.Where(remaining.StartsWith)
                    .OrderByDescending(o => o.Count())
                    .FirstOrDefault();

                if (prefix == null)
                {
                    tokens.Add((Tokens.Unknown, _vocabulary.IndexOf(Tokens.Unknown)));

                    return tokens;
                }

                // Replace the first instance of prefix in remaining
                if (remaining.IndexOf(prefix) == 0)
                {
                    remaining = "##" + remaining.Substring(prefix.Length);
                }

                tokens.Add((prefix, _vocabulary.IndexOf(prefix)));
            }

            if (!string.IsNullOrWhiteSpace(word) && !tokens.Any())
            {
                tokens.Add((Tokens.Unknown, _vocabulary.IndexOf(Tokens.Unknown)));
            }

            return tokens;
        }

        private IEnumerable<string> TokenizeSentence(string text)
        {
            // remove spaces and split the , . : ; etc..
            var cleanText = text.Unidecode();
            var tokens = cleanText.Split(new string[] { " ", "   ", "\r\n" }, StringSplitOptions.None)
                .SelectMany(o => o.SplitAndKeep(".,;:\\/?!#$%()=+-*\"'–_`<>&^@{}[]|~'".ToArray()))
                .Select(o => o.ToLower());
            return tokens;

        }

       
    }
}
