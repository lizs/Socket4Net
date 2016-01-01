using System;

namespace socket4net
{
    /// <summary>
    ///     计数器
    ///     在计数为0时触发回调
    /// </summary>
    public class Counter
    {
        private int _count;
        private readonly Action _action;

        public Counter(Action action)
        {
            _action = action;
        }

        public bool Expired
        {
            get { return _count != 0; }
        }

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
    }
}
