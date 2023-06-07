using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LisaCore.Bot.Conversations
{
    internal class Query
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Input { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;

        [ForeignKey("Conversation")]
        public string ConversationId { get; set; }
        public ICollection<Result> Results { get; set; } = new List<Result>();

        [NotMapped]
        public bool IsSraiInput { get; set; } = false;
    }
}
