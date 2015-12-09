using System;

namespace socket4net
{
    /// <summary>
    /// 基于socket4net逻辑服务的定时器
    /// </summary>
    public class Timer
    {
        public static Timer New(string name, Action action, uint duetime)
        {
            return new Timer(name, action, duetime);
        }

        public static Timer New(string name, Action action, uint duetime, uint period)
        {
            return new Timer(name, action, duetime, period);
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

        private readonly TimerWrapper _timer;
        private readonly Action _action;

        private void OnTimer(TimerWrapper timer)
        {
            _action();
        }

        private Timer(string name, Action action, uint duetime)
        {
            _timer = new TimerWrapper(name, duetime);
            _timer.Arrived += OnTimer;
            _action = action;
        }

        private Timer(string name, Action action, uint duetime, uint period)
        {
            _timer = new TimerWrapper(name, duetime, period);
            _timer.Arrived += OnTimer;
            _action = action;
        }
    }
}
