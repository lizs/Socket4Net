
using System;
using System.Threading;

#if !NET35
using System.Collections.Concurrent;
#else
using Core.ConcurrentCollection;
#endif

namespace Core.Service
{
    public class StaService4Unity : IService
    {
        private readonly ConcurrentQueue<IJob> _jobs = new ConcurrentQueue<IJob>();
        private bool _stopping;
        public int Jobs { get { return _jobs.Count; } }

        public void Startup(int capacity, int period)
        {
        }

        public void Shutdown(bool joinWokerThread)
        {
            _stopping = true;
        }

        public void Perform(Action action)
        {
            _jobs.Enqueue(new Job(action));
        }

        public void Perform<T>(Action<T> action, T param)
        {
            _jobs.Enqueue(new Job<T>(action, param));
        }

        public void Update(float delta)
        {
            if (_stopping) return;

            IJob job;
            while (_jobs.TryDequeue(out job))
            {
                job.Do();
            }
        }
    }
}
