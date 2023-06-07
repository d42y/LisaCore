using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LisaCore.Bot.Conversations
{
    internal class Action
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string ActionText { get; set; }

        [ForeignKey("Result")]
        public string ResultId { get; set; }

        public virtual Result Result { get; set; }
    }
}
