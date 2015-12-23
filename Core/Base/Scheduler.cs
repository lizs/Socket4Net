using System;
using System.Collections.Generic;
using System.Linq;

namespace socket4net
{
    public class Scheduler : IScheduled
    {
        protected string Name;
        public Scheduler(string name)
        {
            Name = name;
        }

        protected readonly Dictionary<Action, TimerWrapper> Timers = new Dictionary<Action, TimerWrapper>();

        public void Destroy()
        {
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

    public class BatchedScheduler : Scheduler
    {
        private readonly IBatched _batched;
        public BatchedScheduler(IBatched batched, string name)
            : base(name)
        {
            _batched = batched;
        }

        public override void InvokeRepeating(Action action, uint delay, uint period)
        {
            CancelInvoke(action);

            var timer = TimerWrapper.New(Name, () =>
            {
                using (new BatchLocker(_batched))
                {
                    action();
                }
            },
                delay, period);
            Timers.Add(action, timer);
            timer.Start();
        }

        public override void Invoke(Action action, uint delay)
        {
            CancelInvoke(action);

            var timer = TimerWrapper.New(Name, () =>
            {
                using (new BatchLocker(_batched))
                {
                    CancelInvoke(action);
                    action();
                }
            }, delay);
            Timers.Add(action, timer);
            timer.Start();
        }
    }
}
