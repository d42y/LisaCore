using BrickSchema.Net;
using BrickSchema.Net.Classes.Points;
using Google.Protobuf.WellKnownTypes;
using LisaCore.Helpers;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace LisaCore.Behaviors.DataAccess
{

    public class HistorizePointInMemory : BrickBehavior, IDisposable
    {

        private readonly object _lock = new object();
        private readonly int _keepDays;


        public HistorizePointInMemory(int keepDays = 90) : base("Historize Point In Memory", typeof(HistorizePointInMemory).Name)
        {
            _keepDays = keepDays;



            BehaviorTimer = null;
        }

        public override void Start()
        {
            if (Parent is BrickSchema.Net.Classes.Point)
            {
                var point = Parent as BrickSchema.Net.Classes.Point;
                point.OnValueChanged += HandleOnParentValueChange;
            }

            // Ensure database is created.
            using (var context = new HistorizePointInMemoryDbContext(Parent.Id))
            {
                context.Database.EnsureCreated();
            }


            // Convert pollRate from seconds to milliseconds, as required by Timer
            int pollRateMilliseconds = 60 * 60 * 1000;

            // Create and start the timer
            BehaviorTimer = new Timer(OnTimerTick, null, pollRateMilliseconds, pollRateMilliseconds);
        }

        public override void Stop()
        {
            if (Parent is BrickSchema.Net.Classes.Point)
            {
                var point = Parent as BrickSchema.Net.Classes.Point;
                point.OnValueChanged -= HandleOnParentValueChange;
            }


            BehaviorTimer?.Dispose();
        }
        private void HandleOnParentValueChange(object? sender, EventArgs e)
        {
            if (Parent is BrickSchema.Net.Classes.Point)
            {
                var point = (BrickSchema.Net.Classes.Point)Parent;
                _logger?.LogInformation($"Historize Point [{point.Name}] Value [{point.Value}]");
                lock (_lock)
                {
                    using (var db = new HistorizePointInMemoryDbContext(point.Id))
                    {
                        var history = db.PointHistories.FirstOrDefault(x => x.Full == false && x.PointId.Equals(point.Id));
                        if (history == null)
                        { //no open history
                            history = new();
                            history.PointId = point.Id;
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
                            var ts = point.Timestamp.ToUniversalTime() - history.EndTimestamp.ToUniversalTime();
                            _logger?.LogInformation($"Historize Point [{point.Name}] Timestamp [{point.Timestamp.ToLocalTime()}]");
                            double interval = Math.Round(Math.Abs(ts.TotalSeconds), 0);
                            _logger?.LogInformation($"Historize Point [{point.Name}] Interval [{interval}]");
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

        public List<(DateTime, double, PointValueQuality)> GetHistory(DateTime startTime, DateTime endTime, int intervalSecond = 0)
        {
            DateTime start = startTime.ToUniversalTime();
            DateTime end = endTime.ToUniversalTime();
            _logger?.LogInformation($"Get History start [{start.ToLocalTime()}] end [{end.ToLocalTime()}] interval [{intervalSecond}]");
            List<(DateTime, double, PointValueQuality)> results = new List<(DateTime, double, PointValueQuality)>();
            List<PointHistory> histories = new();
            if (Parent is BrickSchema.Net.Classes.Point)
            {
                var point = (BrickSchema.Net.Classes.Point)Parent;
                lock (_lock)
                {
                    using (var db = new HistorizePointInMemoryDbContext(point.Id))
                    {
                        histories = db.PointHistories.Where(x => x.PointId.Equals(point.Id)
                                && (x.EndTimestamp >= start
                                    || x.EndTimestamp >= end)).ToList();
                    }
                }
            }
            _logger?.LogInformation(JsonConvert.SerializeObject(histories, Formatting.Indented));
            results = PointHistoryFunctions.BuildHistory(histories, start, end, intervalSecond);

            return results;
        }

        public override void OnTimerTick(object? state)
        {
            if (Parent is BrickSchema.Net.Classes.Point)
            {
                var point = (BrickSchema.Net.Classes.Point)Parent;
                lock (_lock)
                {
                    using (var db = new HistorizePointInMemoryDbContext(point.Id))
                    {
                        // Find all entries that are full and where EndTimestamp is older than _keepDays
                        var entriesToDelete = db.PointHistories.Where(x => x.PointId.Equals(point.Id) && x.Full == true && DateTime.UtcNow - x.EndTimestamp > TimeSpan.FromDays(_keepDays));

                        // Remove these entries from the database
                        db.PointHistories.RemoveRange(entriesToDelete);

                        // Save changes to the database
                        db.SaveChanges();
                    }
                }
            }
        }

        public void Dispose()
        {
            BehaviorTimer?.Dispose();
        }



    }

    internal class HistorizePointInMemoryDbContext : DbContext
    {
        public DbSet<PointHistory> PointHistories { get; set; }

        private readonly string _parentId;

        public HistorizePointInMemoryDbContext(string parentId)
        {
            _parentId = parentId;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase(_parentId);
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
