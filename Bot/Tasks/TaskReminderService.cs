using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LisaCore.Bot.Tasks
{
    internal class TaskReminderService
    {
        private readonly string _directory;
        private Timer _taskCheckTimer;

        public TaskReminderService(string directory)
        {
            _directory = directory;
            Helpers.SystemIOUtilities.CreateDirectoryIfNotExists(directory);

            using (var db = new TaskReminderContext(_directory))
            {
                db.Database.EnsureCreated();
            }
        }

        public async Task<string> AddTaskReminderAsync(string taskDescription, DateTime reminderTime, List<string>? userIds = null)
        {
            using (var db = new TaskReminderContext(_directory))
            {
                TaskStatus taskStatus = userIds?.Count > 0 ? TaskStatus.Open : TaskStatus.NotAssigned;
                var taskReminder = new TaskReminder
                {
                    Id = Guid.NewGuid().ToString(),
                    Description = taskDescription,
                    ReminderTime = reminderTime,
                    Status = taskStatus,
                    LastUpdatedAt = DateTime.UtcNow,
                };

                db.TaskReminders.Add(taskReminder);

                if (userIds != null)
                {
                    foreach (var userId in userIds)
                    {
                        var user = await db.Users.FindAsync(userId);
                        if (user == null)
                        {
                            user = new User { UserId = userId };
                            user.UserName = userId;
                            db.Users.Add(user);
                        }

                        var userTask = new UserTask { UserId = userId, TaskReminderId = taskReminder.Id };
                        db.UserTasks.Add(userTask);
                    }
                }

                await db.SaveChangesAsync();
                return taskReminder.Id;
            }
        }

        public bool RemoveTaskReminder(string reminderId)
        {
            using (var db = new TaskReminderContext(_directory))
            {
                var taskReminder = db.TaskReminders.FirstOrDefault(tr => tr.Id == reminderId);

                if (taskReminder == null) return false;

                db.TaskReminders.Remove(taskReminder);
                db.SaveChanges();
                return true;
            }
        }

        public List<TaskReminder> GetTaskRemindersForUser(string userId)
        {
            using (var db = new TaskReminderContext(_directory))
            {
                return db.UserTasks
                    .Where(ut => ut.UserId == userId)
                    .Select(ut => ut.TaskReminder)
                    .ToList();
            }
        }

        public event EventHandler<TaskReminderEventArgs> TaskReminderNotification;

        protected virtual void OnTaskReminderNotification(TaskReminderEventArgs e)
        {
            TaskReminderNotification?.Invoke(this, e);
        }

        public async Task NotifyUserOfAssignedTasksAsync()
        {
            using (var db = new TaskReminderContext(_directory))
            {
                var currentTime = DateTime.Now;
                var remindersToNotify = db.TaskReminders
                    .Where(tr => tr.ReminderTime <= currentTime && !tr.UserTasks.Any())
                    .ToList();

                foreach (var reminder in remindersToNotify)
                {
                    OnTaskReminderNotification(new TaskReminderEventArgs(reminder));

                    db.TaskReminders.Remove(reminder);
                }

                await db.SaveChangesAsync();
            }
        }

        #region Task Report Functions

        public async Task<TaskReminder?> GetTaskReminderAsync(string id)
        {
            using (var db = new TaskReminderContext(_directory))
            {
                return await db.TaskReminders
                    .FirstOrDefaultAsync(t => t.Id.Equals(id));
            }
        }

        public async Task<List<UserTask>> ListAllTasksAsync (string userId)
        {
            using (var db = new TaskReminderContext(_directory))
            {
                var ut = db.UserTasks
                    .Where(t => t.UserId.Equals(userId)).ToList();
                foreach (var  t in ut)
                {
                    t.TaskReminder = await GetTaskReminderAsync(t.TaskReminderId);
                }
                return ut;
            }
        }
        public int GetTotalTasksForUser(string userId)
        {
            using (var db = new TaskReminderContext(_directory))
            {
                return db.UserTasks.Count(ut => ut.UserId == userId);
            }
        }

        public int GetOverdueTasksForUser(string userId)
        {
            using (var db = new TaskReminderContext(_directory))
            {
                return db.UserTasks.Count(ut => ut.UserId == userId && ut.TaskReminder.Status == TaskStatus.Overdue);
            }
        }

        public int GetOpenTasksForUser(string userId)
        {
            using (var db = new TaskReminderContext(_directory))
            {
                return db.UserTasks.Count(ut => ut.UserId == userId && ut.TaskReminder.Status == TaskStatus.Open);
            }
        }

        public int GetRecentlyClosedTasksForUser(string userId, TimeSpan recentDuration)
        {
            DateTime recentClosedThreshold = DateTime.Now.Subtract(recentDuration);

            using (var db = new TaskReminderContext(_directory))
            {
                return db.UserTasks.Count(ut => ut.UserId == userId && ut.TaskReminder.Status == TaskStatus.Closed && ut.TaskReminder.LastUpdatedAt >= recentClosedThreshold);
            }
        }

        public int GetInProgressTasksForUser(string userId)
        {
            using (var db = new TaskReminderContext(_directory))
            {
                return db.UserTasks.Count(ut => ut.UserId == userId && ut.TaskReminder.Status == TaskStatus.InProgress);
            }
        }
        #endregion 
    }
}
