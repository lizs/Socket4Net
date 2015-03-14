
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Core.Service
{
    /// <summary>
    /// 网络服务线程（网络数据读写）
    /// </summary>
    public static class NetService
    {
        private static BlockingCollection<IJob> _jobs = new BlockingCollection<IJob>();
        private static Thread _wokerThread;
        private static bool _stopping;

        public static int Jobs { get { return _jobs.Count; } }

        public static void Startup()
        {
            _wokerThread = new Thread(WorkingProcedure);
            _wokerThread.Start();
        }

        public static void Shutdown()
        {
            _stopping = true;
            _wokerThread.Join();
        }

        public static void Perform(Action action)
        {
            Enqueue(action);
        }

        public static void Perform<T>(Action<T> action, T param)
        {
            Enqueue(action, param);
        }

        private static void Enqueue<T>(Action<T> proc, T param)
        {
            _jobs.Add(new Job<T>(proc, param));
        }

        private static void Enqueue(Action proc)
        {
            _jobs.Add(new Job(proc));
        }

        private static void WorkingProcedure()
        {
            while (!_stopping)
            {
                IJob job;
                if (_jobs.TryTake(out job, 10))
                {
                    job.Do();
                }
            }
        }
    }
}
