using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LisaCore.Bot.Conversations.Contracts
{
    public class Query
    {
        [Key]
        public string Id { get; set; }
        public string Input { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string ConversationId { get; set; }
        public List<Result> Results { get; set; }

        public Query()
        {
            Id = Guid.NewGuid().ToString();
            Input = string.Empty;
            Timestamp = DateTime.Now;
            Results = new List<Result>();
        }
    }
}
