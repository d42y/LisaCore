using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LisaCore.Bot.Conversations
{
    internal class Result
    {
        [Key]
        public string Id { get; set; }

        public DateTime Timestamp { get; set; }
        
        public string Topic { get; set; }
        public string Template { get; set; }
        public string Match { get; set; }
        public string Message { get; set; }
        public string HtmlMessage { get; set; }

        [ForeignKey("Conversation")]
        public string QueryId { get; set; }


        public virtual List<Action> Actions { get; set; }

        public Result()
        {
            Id = Guid.NewGuid().ToString();
            Timestamp = DateTime.Now;
            Topic = string.Empty;
            Template = string.Empty;
            Match = string.Empty;
            Message = string.Empty;
            HtmlMessage = string.Empty;
            Actions = new List<Action>();
        }
    }
}
