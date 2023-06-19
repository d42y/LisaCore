using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrickSchema.Net;
using LisaCore.Behaviors.Enums;
using LisaCore.Behaviors.Models;
using static LisaCore.Behaviors.DataAccess.ReadPoint;

namespace LisaCore.Behaviors.DataAccess
{
    /// <summary>
    /// Write value back out
    /// </summary>
    public class WritePoint : BrickBehavior
    {
        public delegate bool WritePointFunction(Point point);
        private readonly WritePointFunction _writeFunction;
        private int _pollRate;
        private bool _isExecuting;
        private DateTime _lastExecutionTime;

        public WritePoint(WritePointFunction function) : base(typeof(WritePoint).Name, BehaviorTypes.DataAccess.ToString(), "Write Point Value", 1)
        {
            _writeFunction = function;


        }

        protected override void Load()
        {
            if (Parent?.Type?.Equals(typeof(BrickSchema.Net.Classes.Point).Name) ?? false)
            {
                var point = (BrickSchema.Net.Classes.Point)Parent;
                point.OnValueChanged += OnValueChanged;

            }
        }

        protected override void Unload()
        {

            if (Parent?.Type?.Equals(typeof(BrickSchema.Net.Classes.Point).Name) ?? false)
            {
                var point = (BrickSchema.Net.Classes.Point)Parent;
                point.OnValueChanged -= OnValueChanged;

            }

        }
        public void OnValueChanged(object? sender, EventArgs e)
        {
            if (_writeFunction != null)
            {
                if (Parent?.Type?.Equals(typeof(BrickSchema.Net.Classes.Point).Name) ?? false)
                {
                    var point = (BrickSchema.Net.Classes.Point)Parent;
                    _writeFunction(new() { Id = point.Id, Name = point.Name, Value = point.Value, Timestamp = point.Timestamp, Quality = point.Quality });

                }
            }
        }


    }
}
