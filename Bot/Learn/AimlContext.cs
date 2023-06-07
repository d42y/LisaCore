using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LisaCore.Bot.Learn
{
    internal class AimlContext : DbContext
    {
        private readonly string _connectionString;

        public AimlContext(string directory)
        {
            Helpers.SystemIOUtilities.CreateDirectoryIfNotExists(directory);
            _connectionString = $"Data Source={Path.Combine(directory, "aimlknowledge.db")}";
        }

        public DbSet<AimlCategory> AimlCategories { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(_connectionString);
        }
    }
}
