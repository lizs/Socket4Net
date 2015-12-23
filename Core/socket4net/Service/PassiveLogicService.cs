using System;

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
            if(!_jobs.TryAdd(w))
                Logger.Instance.Error("逻辑服务队列溢出");
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
