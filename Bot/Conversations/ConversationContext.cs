using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LisaCore.Bot.Conversations
{
    internal class ConversationContext : DbContext
    {
        public ConversationContext(DbContextOptions<ConversationContext> options)
            : base(options)
        {
        }

        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Query> Queries { get; set; }
        public DbSet<Result> Results { get; set; }
        public DbSet<Action> Actions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Conversation>().ToTable("Conversations");
            modelBuilder.Entity<Query>().ToTable("Queries");
            modelBuilder.Entity<Result>().ToTable("Results");
            modelBuilder.Entity<Action>().ToTable("Actions");
        }
    }
}
