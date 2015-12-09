
using System;
using Pi.Core.Common;
using Core.Concurrent;

namespace Pi.socket4net.Service
{
    /// <summary>
    /// Unity逻辑服务仅提供一个并发队列
    /// TimerScheduler由Unity提供
    /// </summary>
    public class LogicService4Unity : LogicServiceBase
    {
        private readonly ConcurrentQueue<IJob> _jobs = new ConcurrentQueue<IJob>();
        
        public override void Start()
        {
            StopWorking = false;
            QueueCapacity = Capacity;
            Scheduler = new TimerScheduler(this);
            Watch.Start();
        }

        public override void Stop(bool joinWorker = true)
        {
            StopWorking = true;
            Watch.Stop();
            Scheduler.Dispose();
            Logger.Instance.Debug("Logic service stopped!");
        }

        public override int Jobs
        {
            get { return _jobs.Count; }
        }

        public override void Enqueue(IJob w)
        {
            _jobs.TryAdd(w);
        }

        public void UpdateTimer()
        {
            try
            {
                if (Idle != null)
                    Idle();
            }
            catch (Exception e)
            {
                Logger.Instance.ErrorFormat("{0} : {1}", e.Message, e.StackTrace);
            }
        }

        public void UpdateQueue()
        {
            if (StopWorking)
            {
                try
                {
                    IJob job;
                    while (_jobs.TryDequeue(out job))
                    {
                        job.Do();
                    }
                }
                catch (Exception e)
                {
                    Logger.Instance.ErrorFormat("{0} : {1}", e.Message, e.StackTrace);
                }

                return;
            }

            try
            {
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
            catch (Exception e)
            {
                Logger.Instance.ErrorFormat("{0} : {1}", e.Message, e.StackTrace);
            }
        }

        public override event Action Idle;
    }
}
