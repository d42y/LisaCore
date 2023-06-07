using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LisaCore.Bot.Processor
{
    internal static class StringTransforms
    {
        public static string TransformPerson(string input)
        {
            var replacements = new Dictionary<string, string>
            {
                { "I", "you" },
                { "me", "you" },
                { "my", "your" },
                { "mine", "yours" },
                { "you", "I" },
                { "your", "my" },
                { "yours", "mine" }
            };

            return ReplaceWords(input, replacements);
        }

        public static string TransformPerson2(string input)
        {
            var replacements = new Dictionary<string, string>
            {
                { "I", "he" },
                { "me", "him" },
                { "my", "his" },
                { "mine", "his" },
                { "you", "he" },
                { "your", "his" },
                { "yours", "his" }
            };

            return ReplaceWords(input, replacements);
        }

        public static string TransformGender(string input)
        {
            var replacements = new Dictionary<string, string>
            {
                { "he", "she" },
                { "him", "her" },
                { "his", "her" },
                { "she", "he" },
                { "her", "him" },
                { "hers", "his" }
            };

            return ReplaceWords(input, replacements);
        }

        public static string ToFormalCase(string input)
        {
            var textInfo = new CultureInfo("en-US", false).TextInfo;
            return textInfo.ToTitleCase(input);
        }

        private static string ReplaceWords(string input, Dictionary<string, string> replacements)
        {
            var pattern = $@"\b({string.Join("|", replacements.Keys)})\b";
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);
            return regex.Replace(input, match => replacements[match.Value.ToLower()]);
        }
    }
}
