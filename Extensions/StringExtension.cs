using LisaCore.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LisaCore.Extensions
{
    internal static class StringExtension
    {
        public static IEnumerable<string> SplitAndKeep(
                              this string inputString, params char[] delimiters)
        {
            int start = 0, index;

            while ((index = inputString.IndexOfAny(delimiters, start)) != -1)
            {
                if (index - start > 0)
                    yield return inputString.Substring(start, index - start);

                yield return inputString.Substring(index, 1);

                start = index + 1;
            }

            if (start < inputString.Length)
            {
                yield return inputString.Substring(start);
            }
        }

        public static List<string> GetKeywords(string inputString)
        {
            var keywords = inputString.Split(' ')
                       .Where(keyword => !StopWords.Words.Contains(keyword.ToLower()))
                       .ToList();
            return keywords;
        }
        public static bool ContainsAny(this string str, IEnumerable<string> values)
        {
            return values.Any(str.Contains);
        }
    }
}
