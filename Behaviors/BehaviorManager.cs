using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LisaCore.Behaviors
{
    internal class BehaviorManager
    {
        private readonly Timer timer;
        private readonly object lockObject = new object();
        private readonly List<Action> subscribers = new List<Action>();
        private bool isCallbackInProgress = false;

        public BehaviorManager()
        {
            timer = new Timer(TimerCallback, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
        }

        public void Subscribe(Action subscriber)
        {
            lock (lockObject)
            {
                subscribers.Add(subscriber);
            }
        }

        public void Unsubscribe(Action subscriber)
        {
            lock (lockObject)
            {
                subscribers.Remove(subscriber);
            }
        }

        private void TimerCallback(object state)
        {
            if (isCallbackInProgress)
            {
                // Skip the timer event if a callback is already in progress
                return;
            }

            lock (lockObject)
            {
                if (isCallbackInProgress)
                {
                    // Skip the timer event if a callback is already in progress
                    return;
                }

                isCallbackInProgress = true;
            }

            try
            {
                Action[] currentSubscribers;
                lock (lockObject)
                {
                    currentSubscribers = subscribers.ToArray();
                }

                foreach (var subscriber in currentSubscribers)
                {
                    subscriber.Invoke();
                }
            }
            finally
            {
                lock (lockObject)
                {
                    isCallbackInProgress = false;
                }
            }
        }

    }
}
