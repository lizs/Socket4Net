using System;
using System.Collections.Generic;
using System.Linq;

namespace socket4net
{
    public class Scheduler : Obj
    {
        protected readonly Dictionary<Action, TimerWrapper> Timers = new Dictionary<Action, TimerWrapper>();

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Clear();
        }

        public void Clear()
        {
            foreach (var timer in Timers.Select(x => x.Value))
                timer.Stop();
            Timers.Clear();
        }

        public virtual void InvokeRepeating(Action action, uint delay, uint period)
        {
            CancelInvoke(action);

            var timer = TimerWrapper.New(Name, action, delay, period);
            Timers.Add(action, timer);
            timer.Start();
        }

        public virtual void Invoke(Action action, uint delay)
        {
            CancelInvoke(action);

            var timer = TimerWrapper.New(Name, action, delay);
            Timers.Add(action, timer);
            timer.Start();
        }

        public void CancelInvoke(Action action)
        {
            if (!Timers.ContainsKey(action)) return;

            var timer = Timers[action];
            timer.Stop();
            Timers.Remove(action);
        }
    }
}
