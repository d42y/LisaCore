using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LisaCore.Helpers
{
    internal static class FileReader
    {
        public static List<string> LoadVocabFromFile(string filename)
        {
            var result = new List<string>();

            using (var reader = new StreamReader(filename))
            {
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        result.Add(line);
                    }
                }
            }

            return result;
        }

        public static List<string> LoadVocabFromEmbeddedResource(string path)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream stream = assembly.GetManifestResourceStream(path);
            var result = new List<string>();
            if (stream != null)
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    string line;

                    while ((line = reader.ReadLine()) != null)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            result.Add(line);
                        }
                    }
                }
            }
            else
            {
                // Handle error...
            }
            return result;
        }

        public static Dictionary<int, string> LoadContextFromEmbeddedResource(string path)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream stream = assembly.GetManifestResourceStream(path);
            Dictionary<int, string> sections  = new();
            if (stream != null)
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    var fileContent = reader.ReadToEnd();
                    var sectionArray = fileContent.Split(new[] { "---" }, StringSplitOptions.None);

                    for (int i = 0; i < sectionArray.Length; i++)
                    {
                        sections.Add(i, sectionArray[i].Trim());
                    }
                }
            }
            else
            {
                // Handle error...
            }
            return sections;
        }

        public static Dictionary<int, string> LoadContextFromFile(string filename)
        {
            
            Dictionary<int, string> sections = new();
            
                using (var reader = new StreamReader(filename))
                {
                    var fileContent = reader.ReadToEnd();
                    var sectionArray = fileContent.Split(new[] { "---" }, StringSplitOptions.None);

                    for (int i = 0; i < sectionArray.Length; i++)
                    {
                        sections.Add(i, sectionArray[i].Trim());
                    }
                }
            
            return sections;
        }
    }
}
