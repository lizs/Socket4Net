using System;

namespace socket4net
{/// <summary>
    ///     Batch定时器调度器
    /// </summary>
    public class BatchedScheduler : Scheduler
    {
        private readonly IFlushable _flushable;
        public BatchedScheduler(IFlushable target)
        {
            _flushable = target;
        }

        public override void InvokeRepeating(Action action, uint delay, uint period)
        {
            CancelInvoke(action);

            var timer = TimerWrapper.New("", () =>
            {
                using (new Flusher(_flushable))
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

            var timer = TimerWrapper.New("", () =>
            {
                using (new Flusher(_flushable))
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