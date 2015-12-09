using System;

namespace socket4net
{
    public class BatchLocker : IDisposable
    {
        private readonly AutoLocker _locker;
        public BatchLocker(IBatched batched)
        {
            if(batched.BatchLocker == null) return;
            _locker = new AutoLocker(batched.BatchLocker);
        }

        public void Dispose()
        {
            if(_locker == null) return;
            _locker.Dispose();
        }
    }
}