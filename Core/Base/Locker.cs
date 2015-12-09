using System;

namespace socket4net
{
    /// <summary>
    /// 计数锁
    /// </summary>
    public class Locker : IDisposable
    {
        private int _count;
        private Action _action;
        public Locker(Action action)
        {
            _action = action;
        }

        public bool Locked { get { return _count != 0; } }

        public void Increment()
        {
            ++_count;
        }

        public void Decrement()
        {
            --_count;
            if (_count < 0)
                Logger.Instance.Error("Counter < 0!!!");

            if (_count == 0 && _action != null)
                _action();
        }

        public void Dispose()
        {
            _action = null;
        }
    }

    /// <summary>
    /// 自动锁
    /// </summary>
    public class AutoLocker : IDisposable
    {
        private readonly Locker _locker;
        public AutoLocker(Locker locker)
        {
            _locker = locker;
            _locker.Increment();
        }

        public void Dispose()
        {
            if (_locker != null) _locker.Decrement();
        }
    }
}
