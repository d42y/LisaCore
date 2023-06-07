using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace LisaCore.Bot.Learn
{
    public class Learner
    {
        private readonly string _directory;
        private readonly ILogger? _logger;
        public Learner(string knowledgePath, ILogger? logger = null)
        {
            _logger = logger;
            _directory = knowledgePath;
            Helpers.SystemIOUtilities.CreateDirectoryIfNotExists(_directory);

            using (var db = new AimlContext(_directory))
            {
                db.Database.EnsureCreated();
            }
        }

        public string Directory { get { return _directory; } }

        public async Task DirectLearnAsync(string pattern, string response, string topic = "General")
        {
            await Task.Run(() => DirectLearn(pattern, response, topic));
        }

        private void DirectLearn(string pattern, string response, string topic = "General")
        {
            var aimlCategory = CreateAimlCategory(pattern, response, topic);

            if (aimlCategory != null)
            {
                SaveAimlCategory(aimlCategory);
            }
        }

        private AimlCategory CreateAimlCategory(string pattern, string response, string topic = "General")
        {
            return new AimlCategory
            {
                Pattern = pattern.ToUpperInvariant(),
                Template = response,
                Topic = topic?.ToUpperInvariant() ?? "General".ToUpperInvariant(),
            };
        }

        private void SaveAimlCategory(AimlCategory category)
        {
            using (var db = new AimlContext(_directory))
            {
                var existingCategory = db.AimlCategories.FirstOrDefault(c => c.Pattern == category.Pattern && c.Topic == category.Topic);

                if (existingCategory != null)
                {
                    existingCategory.Template = category.Template;
                }
                else
                {
                    db.AimlCategories.Add(category);
                }

                db.SaveChanges();
            }
        }
    }
}
