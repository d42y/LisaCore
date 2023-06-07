using Microsoft.Recognizers.Text.DateTime;
using Microsoft.Recognizers.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LisaCore.Helpers
{
    internal static class TextRecognizer
    {
        public static System.DateTime? ExtractDateTime (string text)
        {
            var recognizer = new DateTimeRecognizer();
            var culture = Culture.English;
            var model = recognizer.GetDateTimeModel(culture);
            var results = model.Parse(text);

            if (results != null && results.Any())
            {
                var dateTimeResolution = results.First().Resolution;
                var dateTimeValues = dateTimeResolution["values"] as List<Dictionary<string, string>>;

                if (dateTimeValues != null && dateTimeValues.Any())
                {
                    var firstValue = dateTimeValues.First();
                    if (firstValue.ContainsKey("value"))
                    {
                        return System.DateTime.Parse(firstValue["value"]);
                    }
                }
            }

            return null;
        }
    }
}
