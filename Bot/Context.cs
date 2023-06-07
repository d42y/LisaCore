using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LisaCore.Bot
{
    public class Context
    {
        public string UserId { get; set; }
        public Dictionary<string, string> Variables { get; set; }
        public List<string> ConversationHistory { get; set; }

        public Context(string userId)
        {
            UserId = userId;
            Variables = new Dictionary<string, string>();
            ConversationHistory = new List<string>();
        }

        public void SetVariable(string name, string value)
        {
            if (Variables.ContainsKey(name))
            {
                Variables[name] = value;
            }
            else
            {
                Variables.Add(name, value);
            }
        }

        public string GetVariable(string name)
        {
            return Variables.ContainsKey(name) ? Variables[name] : null;
        }

        public void AddToHistory(string message)
        {
            ConversationHistory.Add(message);
        }

        public string GetLastMessage()
        {
            return ConversationHistory.Count > 0 ? ConversationHistory.Last() : null;
        }
    }
}
