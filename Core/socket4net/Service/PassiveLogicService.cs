using System;
#if NET45
using System.Collections.Concurrent;
#endif

namespace socket4net
{
    /// <summary>
    ///     被动逻辑服务
    ///     即：需要上层驱动本服务定时器、Job队列的更新
    /// </summary>
    public class PassiveLogicService : LogicServiceBase
    {
        private readonly ConcurrentQueue<IJob> _jobs = new ConcurrentQueue<IJob>();
        
        protected override void OnStart()
        {
            base.OnStart();

            StopWorking = false;
            Watch.Start();
            Logger.Instance.Debug("Passive logic service started!");
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            StopWorking = true;
            Watch.Stop();
            Logger.Instance.Debug("Logic service stopped!");
        }

        public override int Jobs
        {
            get { return _jobs.Count; }
        }

        public override void Enqueue(IJob w)
        {
            _jobs.Enqueue(w);
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
                Logger.Instance.Exception(e);
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
                    Logger.Instance.Exception(e);
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
                Logger.Instance.Exception(e);
            }
        }

        public override event Action Idle;
    }
}
