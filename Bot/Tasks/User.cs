using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LisaCore.Bot.Tasks
{
    public class User
    {
        public string UserId { get; set; }
        public string UserName { get; set; }

        public ICollection<UserTask> UserTasks { get; set; }
    }
}
