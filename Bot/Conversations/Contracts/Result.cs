using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LisaCore.Bot.Conversations.Contracts
{
    public class Result
    {
        [Key]
        public string Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string QueryId { get; set; }
        public string Topic { get; set; }
        public string Template { get; set; }
        public string Response { get; set; }
        public string HtmlResponse { get; set; }

        public Result()
        {
            Id = Guid.NewGuid().ToString();
            Timestamp = DateTime.Now;
            Topic = "General";
            Template = string.Empty;
            Response = string.Empty;
            HtmlResponse = string.Empty;
        }
    }
}
