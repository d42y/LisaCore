using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LisaCore.Bot.Tasks
{
    public class TaskReminder
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public DateTime ReminderTime { get; set; }
        public ICollection<UserTask> UserTasks { get; set; }
        public TaskStatus Status { get; set; }
        public DateTime LastUpdatedAt { get; set; }

    }
}
