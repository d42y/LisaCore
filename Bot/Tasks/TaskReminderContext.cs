using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LisaCore.Bot.Tasks
{
    internal class TaskReminderContext : DbContext
    {
        private readonly string _connectionString;

        public TaskReminderContext(string directory)
        {
            Helpers.SystemIOUtilities.CreateDirectoryIfNotExists(directory);
            _connectionString = $"Data Source={Path.Combine(directory, "taskreminders.db")}";
        }

        public DbSet<TaskReminder> TaskReminders { get; set; }
        public DbSet<UserTask> UserTasks { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(_connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserTask>()
                .HasKey(ut => new { ut.UserId, ut.TaskReminderId });

            modelBuilder.Entity<UserTask>()
                .HasOne(ut => ut.User)
                .WithMany(u => u.UserTasks)
                .HasForeignKey(ut => ut.UserId);

            modelBuilder.Entity<UserTask>()
                .HasOne(ut => ut.TaskReminder)
                .WithMany(tr => tr.UserTasks)
                .HasForeignKey(ut => ut.TaskReminderId);
        }
    }
}
