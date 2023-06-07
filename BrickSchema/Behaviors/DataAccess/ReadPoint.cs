using Mages.Core.Runtime.Converters;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickSchema.Net.Behaviors.DataAccess
{
    /// <summary>
    /// Read point and write to point entity
    /// </summary>
    public class ReadPoint : BrickBehavior, IDisposable
    {
        // Define a delegate type for read operations
        public delegate LisaCore.BrickSchema.Models.Point? ReadPointFunction(LisaCore.BrickSchema.Models.Point point);
        private readonly ReadPointFunction _readFunction;
        
        

        public ReadPoint(ReadPointFunction function, int pollRate = 60) : base("Read Point Value", typeof(ReadPoint).Name)
        {
            _readFunction = function;
            PollRate = pollRate;
            BehaviorTimer = null;
        }

        public override void OnTimerTick(object? state)
        {
            // Call the read function with the parent's identity
            if (Parent is Classes.Point)
            {
                var point = Parent as Classes.Point;


                var newPoint = _readFunction(new() { Id = point.Id, Name = point.Name, Value = point.Value, Timestamp = point.Timestamp, Quality=point.Quality});
                if (newPoint.Id.Equals(Parent.Id))
                {
                    point.UpdateValue(newPoint.Value, newPoint.Timestamp, newPoint.Quality);
                }
            }
        }

        public override void Start()
        {
            // Convert pollRate from seconds to milliseconds, as required by Timer
            int pollRateMilliseconds = PollRate * 1000;

            // Create and start the timer
            BehaviorTimer = new Timer(OnTimerTick, null, pollRateMilliseconds, pollRateMilliseconds);
        }

        /// <summary>
        /// Optionally, you might want a method to stop the timer
        /// </summary>
        public override void Stop()
        {
            BehaviorTimer?.Dispose();
        }

        public void Dispose()
        {
            BehaviorTimer?.Dispose();
        }
    }
}
