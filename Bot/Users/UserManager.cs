using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LisaCore.Bot.Users
{
    internal class UserManager
    {
        private readonly string _dbDirectory;
        private readonly string _userId;
        private readonly SemaphoreSlim _semaphore;
        private readonly ILogger? _logger;
        public UserManager(string userId, string dbDirectory, ILogger? logger = null)
        {
            _logger = logger;
            
            _dbDirectory = dbDirectory;

            _userId = userId;
            _semaphore = new SemaphoreSlim(1, 1);

            InitializeDatabase();
        }

        public string UserId { get { return _userId; } }

        private SqliteConnection GetConnection()
        {
            Helpers.SystemIOUtilities.CreateDirectoryIfNotExists(_dbDirectory);
            var sanitizedUserId = Helpers.SystemIOUtilities.SanitizeFileName(_userId);
            var connectionString = $"Data Source={Path.Combine(_dbDirectory, $"{sanitizedUserId}_UserVariables.db")};";
            return new SqliteConnection(connectionString);
        }

        private void InitializeDatabase()
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS variables (name TEXT PRIMARY KEY, value TEXT);", conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public async Task<string> GetVariableAsync(string variableName)
        {
            await _semaphore.WaitAsync();

            try
            {
                using (var conn = GetConnection())
                {
                    await conn.OpenAsync();

                    using (var cmd = new SqliteCommand("SELECT value FROM variables WHERE name = $name;", conn))
                    {
                        cmd.Parameters.AddWithValue("$name", variableName);

                        var result = await cmd.ExecuteScalarAsync();
                        return result?.ToString();
                    }
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task SetVariableAsync(string variableName, string value)
        {
            await _semaphore.WaitAsync();

            try
            {
                using (var conn = GetConnection())
                {
                    await conn.OpenAsync();

                    using (var cmd = new SqliteCommand("INSERT OR REPLACE INTO variables (name, value) VALUES ($name, $value);", conn))
                    {
                        cmd.Parameters.AddWithValue("$name", variableName);
                        cmd.Parameters.AddWithValue("$value", value);

                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        
    }
}
