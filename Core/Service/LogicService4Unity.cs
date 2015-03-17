
using System;
using System.Threading;
using Core.Timer;
#if !NET35
using System.Collections.Concurrent;
#else
using Core.Concurrent;
#endif

namespace Core.Service
{
    public class LogicService4Unity : ILogicService
    {
        private readonly ConcurrentQueue<IJob> _jobs = new ConcurrentQueue<IJob>();
        private bool _stopping = true;
        public int Jobs { get { return _jobs.Count; } }
        public int ExcutedJobsPerSec { get; private set; }
        public int Capacity { get; set; }
        public int Period { get; set; }
        
        public void Start()
        {
            _stopping = false;
        }

        public void Stop(bool joinWorker = true)
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
            if (_jobs.TryDequeue(out job))
            {
                job.Do();
            }
            else
            {
                if (Idle != null)
                    Idle();
            }
        }

        public event Action Idle;

        /// <summary>
        /// not used in unity
        /// </summary>
        public TimerScheduler Scheduler { get; private set; }
    }
}
