
using System;
using System.Threading;

#if !NET35
using System.Collections.Concurrent;
#else
using Core.ConcurrentCollection;
#endif

namespace Core.Service
{
    /// <summary>
    /// 网络服务线程（网络数据读写）
    /// </summary>
    public class NetService : IService
    {
        private BlockingCollection<IJob> _jobs;
        private Thread _wokerThread;
        private bool _stopping;

        public int Jobs { get { return _jobs.Count; } }
        public int Capacity { get; set; }

        private int _period = 10;
        public int Period
        {
            get { return _period; }
            set { _period = value; }
        }

        public void Start()
        {
            _jobs = new BlockingCollection<IJob>(Capacity);

            _wokerThread = new Thread(WorkingProcedure);
            _wokerThread.Start();
        }

        public void Stop(bool joinWorker = true)
        {
            _stopping = true;
            if(joinWorker)
                _wokerThread.Join();
        }

        public void Perform(Action action)
        {
            Enqueue(action);
        }

        public void Perform<T>(Action<T> action, T param)
        {
            Enqueue(action, param);
        }

        private void Enqueue<T>(Action<T> proc, T param)
        {
            _jobs.Add(new Job<T>(proc, param));
        }

        private void Enqueue(Action proc)
        {
            _jobs.Add(new Job(proc));
        }

        private void WorkingProcedure()
        {
            while (!_stopping)
            {
                IJob job;
                if (_jobs.TryTake(out job, Period))
                {
                    job.Do();
                }
            }
        }
    }
}
