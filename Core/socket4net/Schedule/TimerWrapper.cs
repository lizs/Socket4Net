using System;

namespace socket4net
{
    /// <summary>
    /// 基于socket4net逻辑服务的定时器
    /// </summary>
    public class TimerWrapper
    {
        public static TimerWrapper New(string name, Action action, uint duetime)
        {
            return new TimerWrapper(name, action, duetime);
        }

        public static TimerWrapper New(string name, Action action, uint duetime, uint period)
        {
            return new TimerWrapper(name, action, duetime, period);
        }

        public void Start()
        {
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Arrived -= OnTimer;
            _timer.Stop();
            _timer.Dispose();
        }

        private readonly Timer _timer;
        private readonly Action _action;

        private void OnTimer(Timer timer)
        {
            _action();
        }

        private TimerWrapper(string name, Action action, uint duetime)
        {
            _timer = new Timer(name, duetime);
            _timer.Arrived += OnTimer;
            _action = action;
        }

        private TimerWrapper(string name, Action action, uint duetime, uint period)
        {
            _timer = new Timer(name, duetime, period);
            _timer.Arrived += OnTimer;
            _action = action;
        }
    }
}
