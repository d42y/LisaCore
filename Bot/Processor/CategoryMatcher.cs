using LisaCore.Bot.Learn;
using LisaCore.Nlp.BERT;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LisaCore.Bot.Processor
{
    internal class CategoryMatcher
    {
        private readonly Dictionary<string, XElement> _categories = new Dictionary<string, XElement>();
        private Dictionary<int, string> _NlpContext { get; set; } = new();

        private string? _dbDirectory = null;
        private readonly ILogger? _logger;
        public CategoryMatcher(ILogger? logger = null)
        {
            _logger = logger;
        }

        public void SetNlpConext (Dictionary<int, string> context)
        {
            _NlpContext.Clear();
            _NlpContext = context;
        }
        public void LoadLearnedKnowledge(string directory)
        {
            _dbDirectory = directory;
        }

        public void LoadTrainedKnowledge(string knowledgeDirectory)
        {
            if (!Directory.Exists(knowledgeDirectory))
            {
                throw new ArgumentException("The provided knowledge directory does not exist.");
            }

            var aimlFiles = Directory.GetFiles(knowledgeDirectory, "*.aiml", SearchOption.AllDirectories);
            if (aimlFiles.Length == 0)
            {
                return;
                //throw new ArgumentException("The provided knowledge directory does not contain any .aiml files.");
            }

            foreach (var aimlFile in aimlFiles)
            {
                try
                {
                    var xDocument = XDocument.Load(aimlFile);
                    LoadCategories(xDocument);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading AIML file: {ex.Message}");
                }
            }
        }

        public void LoadSystemKnowledge()
        {
            LoadAimlFromEmbeddedResource("LisaCore.lib.aiml.ahu.aiml");
            LoadAimlFromEmbeddedResource("LisaCore.lib.aiml.chiller.aiml");
            LoadAimlFromEmbeddedResource("LisaCore.lib.aiml.chws.aiml");
            LoadAimlFromEmbeddedResource("LisaCore.lib.aiml.defaultsetpoints.aiml");
            LoadAimlFromEmbeddedResource("LisaCore.lib.aiml.dictionary.aiml");
            //LoadAimlFromEmbeddedResource("LisaCore.lib.aiml.emotion.aiml");
            LoadAimlFromEmbeddedResource("LisaCore.lib.aiml.geography.aiml");
            LoadAimlFromEmbeddedResource("LisaCore.lib.aiml.hello.aiml");
            LoadAimlFromEmbeddedResource("LisaCore.lib.aiml.hvac.aiml");
            LoadAimlFromEmbeddedResource("LisaCore.lib.aiml.smartbuilding.aiml");
            LoadAimlFromEmbeddedResource("LisaCore.lib.aiml.tasks.aiml");
            LoadAimlFromEmbeddedResource("LisaCore.lib.aiml.vav.aiml");
        }

        internal (List<string>, float?) AskBert(Bert bert, string input)
        {
            if (bert != null)
            {
                return bert.Predict(_NlpContext, input);
            }
            return (new(), null);
        }

        private void LoadAimlFromEmbeddedResource(string path)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream stream = assembly.GetManifestResourceStream(path);

            if (stream != null)
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    string content = reader.ReadToEnd();
                    try
                    {
                        var xDocument = XDocument.Parse(content);
                        LoadCategories(xDocument);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error loading AIML file: {ex.Message}");
                    }
                }
            }
            else
            {
                // Handle error...
            }
        }

        public void LoadCategories(XDocument xDocument)
        {
            var topics = xDocument.Descendants("topic");

            if (!topics.Any())
            {
                topics = new List<XElement> { new XElement("topic", xDocument.Root.Elements("category")) };
            }

            foreach (var topic in topics)
            {
                var topicName = topic.Attribute("name")?.Value.ToUpperInvariant() ?? "*";
                var categories = topic.Elements("category");

                foreach (var category in categories)
                {
                    var pattern = category.Element("pattern")?.Value.ToUpperInvariant();

                    if (!string.IsNullOrWhiteSpace(pattern))
                    {
                        _categories[$"{topicName}:{pattern}"] = category;
                        _NlpContext.Add(_NlpContext.Count + 1, $"{topicName}:{pattern} {category.Element("template")?.Value}");
                    }
                }
            }
        }

        public XElement FindMatch(string input)
        {
            XElement? match = FindMatchInDatabase(input); 
            if (match == null)
            {
                match = FindMatchInDictionary(input);
            }

            return match;
        }

        private XElement FindMatchInDictionary(string input)
        {
            return _categories.FirstOrDefault(x => IsPatternMatch(input, x.Key)).Value;
        }

        private XElement? FindMatchInDatabase(string input)
        {
            try
            {
                if (_dbDirectory == null) return null;
                using (var db = new AimlContext(_dbDirectory))
                {
                    var aimlCategories = db.AimlCategories.ToList(); // Fetch all AimlCategory records

                    foreach (var aimlCategory in aimlCategories)
                    {
                        var key = $"*:{aimlCategory.Pattern}";
                        if (IsPatternMatch(input, key))
                        {
                            return new XElement("category",
                                new XElement("pattern", aimlCategory.Pattern),
                                new XElement("template", aimlCategory.Template)
                            );
                        }
                    }
                }
            } catch { }
            return null;
        }

        private bool IsPatternMatch(string input, string key)
        {
            var pattern = GetPatternFromKey(key);
            pattern = Regex.Escape(pattern);
            pattern = pattern.Replace("\\*", "(.+)");
            return Regex.IsMatch(input, $"^{pattern}$", RegexOptions.IgnoreCase);
        }

        private string GetPatternFromKey(string key)
        {
            return key.Split(new[] { ':' }, 2)[1];
        }


        private double JaccardSimilarity(string source, string target)
        {
            var sourceTokens = new HashSet<string>(source.Split(' '));
            var targetTokens = new HashSet<string>(target.Split(' '));

            var intersection = sourceTokens.Intersect(targetTokens).Count();
            var union = sourceTokens.Union(targetTokens).Count();

            return union == 0 ? 0 : (double)intersection / union;
        }

        private int LevenshteinDistance(string source, string target)
        {
            if (string.IsNullOrEmpty(source))
                return string.IsNullOrEmpty(target) ? 0 : target.Length;

            if (string.IsNullOrEmpty(target))
                return source.Length;

            var matrix = new int[source.Length + 1, target.Length + 1];

            for (int i = 0; i <= source.Length; i++)
                matrix[i, 0] = i;

            for (int j = 0; j <= target.Length; j++)
                matrix[0, j] = j;

            for (int i = 1; i <= source.Length; i++)
                for (int j = 1; j <= target.Length; j++)
                    matrix[i, j] = Math.Min(Math.Min(
                        matrix[i - 1, j] + 1,
                        matrix[i, j - 1] + 1),
                        matrix[i - 1, j - 1] + (source[i - 1] == target[j - 1] ? 0 : 1));

            return matrix[source.Length, target.Length];
        }

        private double CalculateCombinedScore(string input, string pattern)
        {
            double jaccardWeight = 0.5;
            double levenshteinWeight = 1 - jaccardWeight;

            double jaccardScore = JaccardSimilarity(input, pattern);
            double levenshteinScore = 1.0 - (double)LevenshteinDistance(input, pattern) / Math.Max(input.Length, pattern.Length);

            return jaccardWeight * jaccardScore + levenshteinWeight * levenshteinScore;
        }

        public (XElement?, double) FindClosestMatch(string input)
        {
            double highestScore = 0;
            XElement? closestCategory = null;

            if (_dbDirectory != null)
            {
                // Search SQLite database
                using (var db = new AimlContext(_dbDirectory))
                {
                    var aimlCategories = db.AimlCategories.ToList(); // Fetch all AimlCategory records

                    foreach (var aimlCategory in aimlCategories)
                    {
                        double combinedScore = CalculateCombinedScore(input, aimlCategory.Pattern);
                        if (combinedScore > highestScore && combinedScore >= 0.50)
                        {
                            highestScore = combinedScore;
                            closestCategory = new XElement("category",
                                new XElement("pattern", aimlCategory.Pattern),
                                new XElement("template", aimlCategory.Template)
                            );
                        }
                    }
                }
            }

            // Search _categories dictionary
            foreach (var category in _categories.Values)
            {
                var pattern = category.Element("pattern")?.Value.ToUpperInvariant();
                if (pattern != null)
                {
                    double combinedScore = CalculateCombinedScore(input, pattern);
                    if (combinedScore > highestScore && combinedScore >= 0.50)
                    {
                        highestScore = combinedScore;
                        closestCategory = category;
                    }
                }
            }

           

            return (closestCategory, highestScore);
        }



    }
}
