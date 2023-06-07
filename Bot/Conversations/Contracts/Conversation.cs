using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LisaCore.Bot.Conversations.Contracts
{
    public class Conversation
    {
        [Key]
        public string UserId { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime Timestamp { get; set; }

        public Conversation()
        {
            Id = Guid.NewGuid().ToString();
            Name = string.Empty;
            Timestamp = DateTime.Now;
        }
    }
}
