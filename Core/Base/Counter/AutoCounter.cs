using System;

namespace socket4net
{
    /// <summary>
    ///     自动计数器
    ///     注：方便在using块中使用
    /// </summary>
    public class AutoCounter : IDisposable
    {
        private readonly Counter _counter;

        public AutoCounter(Counter counter)
        {
            _counter = counter;
            _counter.Increment();
        }

        public void Dispose()
        {
            if (_counter != null) _counter.Decrement();
        }
    }
}