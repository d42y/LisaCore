using Mages.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LisaCore.Bot.Processor
{
    internal static class MathProcessor
    {
        public static bool IsMathExpression(string input)
        {
            try
            {
                var engine = new Engine();
                engine.Interpret(input); // If this does not throw an exception, it's a valid expression
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static double? ProcessMathExpression(string input)
        {
            if (!IsMathExpression(input)) return null;

            try
            {
                var engine = new Engine();
                var result = engine.Interpret(input);
                if (result == null) return null;
                return Convert.ToDouble(result);
            }
            catch
            {
                // Handle or log exception here
                return null;
            }
        }
    }
}
