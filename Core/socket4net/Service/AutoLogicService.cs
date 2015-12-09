/********************************************************************
 *  created:    2011/12/17   6:17
 *  filename:   StaService.cs
 *
 *  author:     Linguohua
 *  copyright(c) 2011
 *
 *  purpose:    implement StaService class. StaService represents single thread
 *      apartment.  It's a work items queue plus an Idle event,
 *      it consumes work items or call Idle event when it's arrive
 *      it's period.
*********************************************************************/

using System;
using System.Threading;

namespace socket4net
{
    /// <summary>
    /// StaService is a component provides a producer/consumer
    /// pattern working queue, thus translate multiple
    /// threads model into single thread model.
    ///     自动逻辑服务
    ///     定时器刷新以及Jobs队列的刷新完全由本模块自理
    /// </summary>
    public class AutoLogicService : LogicServiceBase
    {
        private Thread _workingThread;
        private BlockingCollection<IJob> _workingQueue;

        public override void Start()
        {
            if (_workingQueue != null
                || _workingThread != null)
            {
                Logger.Instance.Fatal("AutoLogicService being start more than once");
                return;
            }

            QueueCapacity = Capacity;
            Scheduler = new TimerScheduler(this);
            DoStartup();

            Logger.Instance.Debug("Auto logic service started!");
        }

        public override void Stop(bool joinWorker = true)
        {
            Scheduler.Dispose();

            if (_workingThread == null
                || _workingQueue == null)
            {
                Logger.Instance.Fatal("AutoLogicService not yet been started");
                return;
            }

            StopWorking = true;
            if (joinWorker)
            {
                _workingThread.Join();
            }

            Logger.Instance.Debug("Logic service stopped!");
        }

        public override event Action Idle;

        /// <summary>
        /// Specify the work items count currently in working queue.
        /// </summary>
        public override int Jobs
        {
            get { return _workingQueue.Count; }
        }
        
        /// <summary>
        /// External(e.g. the TCP socket thread) call this method to push
        /// a work item into the working queue. The work item must not
        /// be null.
        /// </summary>
        /// <param name="w">the work item object, must not be null</param>
        public override void Enqueue(IJob w)
        {
            if (!_workingQueue.TryAdd(w, 0))
            {
                _workingQueue.Add(w);
            }
        }
   
        /// <summary>
        /// do actually startup job
        /// </summary>
        private void DoStartup()
        {
            _workingQueue = new BlockingCollection<IJob>(QueueCapacity);
            _workingThread = new Thread(WorkingProcedure) { Name = "AutoLogicService", IsBackground = true };

            // use background thread
            // see http://msdn.microsoft.com/en-us/library/h339syd0.aspx

            _workingThread.Start();
        }
        
        /// <summary>
        /// working thread's working procedure
        /// </summary>
        private void WorkingProcedure()
        {
            Watch.Start();

            var handled = 0;
            while (!StopWorking)
            {
                var periodCounter = StopWatchDivider;
                var tick = Environment.TickCount;

                var t1 = Watch.ElapsedMilliseconds;

                do
                {
                    try
                    {
                        IJob item;
                        if (_workingQueue.TryTake(out item, Period))
                        {
                            item.Do();
                            CalcPerformance();

                            handled++;
                            periodCounter--;

                            if (periodCounter < 1)
                            {
                                if ((Watch.ElapsedMilliseconds - t1) >= Period)
                                {
                                    break;
                                }
                                periodCounter = StopWatchDivider;
                            }
                        }
                        else
                            break;
                    }
                    catch (Exception ex)
                    {   
                        Logger.Instance.FatalFormat("{0} : {1}", ex.Message, ex.StackTrace);
                    }

                }
                while ((Environment.TickCount - tick) < Period);

                WiElapsed = Watch.ElapsedMilliseconds - t1;

                if (Idle != null)
                {
                    var t2 = Watch.ElapsedMilliseconds;
                    try
                    {
                        Idle();
                    }
                    catch (Exception ex)
                    {
                        Logger.Instance.Fatal(string.Format("{0} : {1}", ex.Message, ex.StackTrace));
                    }

                    IdleCallbackElapsed = Watch.ElapsedMilliseconds - t2;
                }
            }

            
            IJob leftItem;
            while (_workingQueue.TryTake(out leftItem, Period * 10))
            {
                leftItem.Do();
            }

            Watch.Stop();
        }
    }
}
