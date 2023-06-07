using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LisaCore.Bot.Tasks
{
    public class TaskReminderEventArgs : EventArgs
    {
        public TaskReminderEventArgs(TaskReminder taskReminder)
        {
            TaskReminder = taskReminder;
        }

        public TaskReminder TaskReminder { get; }
    }
}
