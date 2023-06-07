using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeCantSpell.Hunspell;

namespace LisaCore.Bot.Languages
{
    public class SpellChecker
    {
        private readonly WordList _wordList;

        public SpellChecker(string affFilePath, string dicFilePath)
        {
            _wordList = WordList.CreateFromFiles(affFilePath, dicFilePath);
        }

        public string CheckAndCorrect(string input)
        {
            var words = input.Split(' ');
            var correctedWords = new List<string>();

            foreach (var word in words)
            {
                if (!_wordList.Check(word))
                {
                    var suggestions = _wordList.Suggest(word).ToList(); // Convert to list
                    correctedWords.Add(suggestions.Count > 0 ? suggestions[0] : word);
                }
                else
                {
                    correctedWords.Add(word);
                }
            }

            return string.Join(" ", correctedWords);
        }
    }
}
