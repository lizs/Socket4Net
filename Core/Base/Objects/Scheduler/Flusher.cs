using System;

namespace socket4net
{
    public interface IFlushable : IObj
    {
        void Flush();
    }

    public class Flusher : IDisposable
    {
        private IFlushable _target;
        public Flusher(IFlushable target)
        {
            _target = target;
        }

        public void Dispose()
        {
            if (_target == null) return;

            _target.Flush();
            _target = null;
        }
    }
}