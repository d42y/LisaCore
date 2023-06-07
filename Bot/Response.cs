using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LisaCore.Bot
{
    internal class Response
    {
        public DateTime Timestamp { get; set; }
       
        public string Topic { get; set; }
        public XElement? Template { get; set; }
        public Match? Match {get; set;}
        public string Message { get; set; }
        public string HtmlMessage { get; set; }
        public List<string> Actions { get; set; }
        public bool IsError { get; set; }
        public Response()
        {
            Timestamp = DateTime.Now;
            Topic = string.Empty;
            Template = null;
            Match = null;
            Message = string.Empty;
            HtmlMessage = string.Empty;
            Actions = new List<string>();
            IsError = false;
        }

        public void AddAction(string action)
        {
            Actions.Add(action);
        }

        public void ExecuteActions()
        {
            // Implement your action execution logic here
        }

    }
}
