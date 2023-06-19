using BrickSchema.Net;
using LisaCore.Behaviors.Enums;
using Mages.Core.Runtime.Converters;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LisaCore.Behaviors.DataAccess
{
    /// <summary>
    /// Read point and write to point entity
    /// </summary>
    public class ReadPoint : BrickBehavior
    {
        // Define a delegate type for read operations
        public delegate Models.Point? ReadPointFunction(Models.Point point);
        private readonly ReadPointFunction _readFunction;
        private int _pollRate;
        private bool _isExecuting;
        private DateTime _lastExecutionTime;


        public ReadPoint(ReadPointFunction function, int pollRate = 60) : 
            base(typeof(ReadPoint).Name,BehaviorTypes.DataAccess.ToString(), "Read Point Value", 1)
        {
            _readFunction = function;
            _pollRate = pollRate;
            _isExecuting = false;
            _lastExecutionTime = DateTime.Now;
        }

        protected override void Execute()
        {
            if (_lastExecutionTime.AddSeconds(_pollRate) > DateTime.Now || _isExecuting) { return; }

            _isExecuting = true;
            try
            {
                // Call the read function with the parent's identity
                if (Parent is BrickSchema.Net.Classes.Point)
                {
                    var point = Parent as BrickSchema.Net.Classes.Point;


                    var newPoint = _readFunction(new() { Id = point.Id, Name = point.Name, Value = point.Value, Timestamp = point.Timestamp, Quality = point.Quality });
                    if (newPoint.Id.Equals(Parent.Id))
                    {
                        point.UpdateValue(newPoint.Value, newPoint.Timestamp, newPoint.Quality);
                    }
                }
            } catch (Exception ex)
            {
                _isExecuting = false;
                throw;
            }
            _isExecuting = false;
            _lastExecutionTime = DateTime.Now;
            
        }

        
    }
}
