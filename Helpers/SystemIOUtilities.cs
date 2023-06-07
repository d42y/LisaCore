using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LisaCore.Helpers
{
    internal static class SystemIOUtilities
    {
        public static void CreateDirectoryIfNotExists(string directoryPath)
        {
            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating directory '{directoryPath}': {ex.Message}");
            }
        }

        public static string SanitizeFileName(string fileName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var charMap = new Dictionary<char, char>
        {
            { '\\', 'A' },
            { '/', 'B' },
            { ':', 'C' },
            { '*', 'D' },
            { '?', 'E' },
            { '\"', 'F' },
            { '<', 'G' },
            { '>', 'H' },
            { '|', 'I' }
        };

            var sanitizedFileName = new string(fileName.Select(ch => invalidChars.Contains(ch) ? charMap[ch] : ch).ToArray());
            return sanitizedFileName;
        }
    }
}
