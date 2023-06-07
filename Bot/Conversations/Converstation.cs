using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LisaCore.Bot.Conversations
{
    internal class Conversation
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string Name { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public ICollection<Query> Queries { get; set; } = new List<Query>();
    }
}
