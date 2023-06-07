using BrickSchema.Net.Classes.Points;
using LisaCore.Helpers;
using MachineLearningLibrary.Functions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace BrickSchema.Net.Behaviors.DataAccess
{

    public class HistorizePointSqlite :BrickBehavior, IDisposable 
    {
        private string _connectionString;
        private readonly object _lock = new object();
        private readonly int _keepDays;
        private readonly string _dbDirectory;
        public HistorizePointSqlite(string directory, int keepDays = 730) : base("Historize Point In Sqlite", typeof(HistorizePointSqlite).Name)
        {
            _keepDays = keepDays;
            LisaCore.Helpers.SystemIOUtilities.CreateDirectoryIfNotExists(directory);
            _dbDirectory = directory;

            
            BehaviorTimer = null;
        }


        public override void Start()
        {
            string databaseFilePath = Path.Combine(_dbDirectory, $"{Parent.Id}.db");
            _connectionString = $"Data Source={databaseFilePath}";
            // Ensure database is created.
            using (var context = new HistorizePointSqliteDbContext(_connectionString))
            {
                context.Database.EnsureCreated();
            }

            if (Parent?.Type?.Equals(typeof(BrickSchema.Net.Classes.Point).Name) ?? false)
            {
                var point = (Classes.Point)Parent;
                point.OnValueChanged += OnValueChanged;

            }
            // Convert pollRate from seconds to milliseconds, as required by Timer
            int pollRateMilliseconds = 60 * 60 * 1000;

            // Create and start the timer
            BehaviorTimer = new Timer(OnTimerTick, null, pollRateMilliseconds, pollRateMilliseconds);
        }

        public override void Stop()
        {
            if (Parent is Classes.Point)
            {
                var point = (Classes.Point)Parent;
                point.OnValueChanged -= OnValueChanged;

            }
            BehaviorTimer?.Dispose();
        }

        private void OnValueChanged(object? sender, EventArgs e)
        {
            if (Parent is Classes.Point)
            {
                var point = (Classes.Point)Parent;

                lock (_lock)
                {
                    using (var db = new HistorizePointSqliteDbContext(_connectionString))
                    {
                        var history = db.PointHistories.FirstOrDefault(x => x.Full == false && x.PointId.Equals(point.Id));
                        if (history == null)
                        { //no open history
                            history = new();
                            history.Values.Add(point.Value ?? 0.0);
                            history.Intervals.Add(0);
                            history.Qualities.Add(point.Quality);
                            history.StartTimestamp = point.Timestamp.ToUniversalTime();
                            history.EndTimestamp = point.Timestamp.ToUniversalTime();
                            db.PointHistories.Add(history);

                        }
                        else
                        {//Insert

                            history.Values.Add(point.Value ?? 0.0);
                            var ts = point.Timestamp.ToUniversalTime() - history.EndTimestamp;
                            double interval = Math.Round(Math.Abs(ts.TotalSeconds), 0);
                            history.Intervals.Add(interval);
                            history.Qualities.Add(point.Quality);
                            history.EndTimestamp = point.Timestamp.ToUniversalTime();
                            history.Full = history.Values.Count >= 1440;
                            var local = db.Set<PointHistory>().Local.FirstOrDefault(e => e.Id.Equals(history.Id));

                            //check if local is not null
                            if (local != null)
                            {
                                //detach
                                db.Entry(local).State = EntityState.Detached;
                            }
                            db.Entry(history).State = EntityState.Modified;

                        }
                        db.SaveChanges();

                    }
                }
            }
        }

        public List<(DateTime, double, PointValueQuality)> GetHistory(DateTime startTime, DateTime endTime, int interval = 0)
            {
                List<(DateTime, double, PointValueQuality)> results = new List<(DateTime, double, PointValueQuality)>();
                List<PointHistory> histories = new();
            if (Parent is Classes.Point)
            {
                var point = (Classes.Point)Parent;
                lock (_lock)
                {

                    using (var db = new HistorizePointSqliteDbContext(_connectionString))
                    {
                        histories = db.PointHistories.Where(x => x.PointId.Equals(point.Id)
                                && (x.EndTimestamp >= startTime
                                    || x.EndTimestamp >= endTime)).ToList();



                    }
                }
            }

                results = PointHistoryFunctions.BuildHistory(histories, startTime, endTime, interval);
            
            return results;
        }

        public override void OnTimerTick(object? state)
        {
            lock (_lock)
            {
                using (var db = new HistorizePointSqliteDbContext(_connectionString))
                {
                    // Find all entries that are full and where EndTimestamp is older than _keepDays
                    var entriesToDelete = db.PointHistories.Where(x => x.Full == true && DateTime.UtcNow - x.EndTimestamp > TimeSpan.FromDays(_keepDays));

                    // Remove these entries from the database
                    db.PointHistories.RemoveRange(entriesToDelete);

                    // Save changes to the database
                    db.SaveChanges();
                }
            }
        }

        public void Dispose()
        {
            BehaviorTimer?.Dispose();
        }

        

    }

    internal class HistorizePointSqliteDbContext : DbContext
    {
        public DbSet<PointHistory> PointHistories { get; set; }

        private readonly string _connectionString;

        public HistorizePointSqliteDbContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(_connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<PointHistory>()
                .Property(e => e.Values)
                .HasConversion(new RunLengthEncodingConverter<double>());

            modelBuilder.Entity<PointHistory>()
               .Property(e => e.Intervals)
               .HasConversion(new RunLengthEncodingConverter<double>());

            modelBuilder.Entity<PointHistory>()
                .Property(e => e.Qualities)
                .HasConversion(new RunLengthEncodingConverter<PointValueQuality>());
        }
    }
}
