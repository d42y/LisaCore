using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LisaCore.Bot.Tasks
{
    public class UserTask
    {
        public string UserId { get; set; }
        public string TaskReminderId { get; set; }
        public User User { get; set; }
        public TaskReminder TaskReminder { get; set; }

        public override string ToString()
        {
            try
            {
                string task = $"Task {TaskReminder.Description} is {TaskReminder.Status} and due on {TaskReminder.ReminderTime}";
                return task;
            }
            catch (Exception ex) { }

            return string.Empty;
        }
    }
}
