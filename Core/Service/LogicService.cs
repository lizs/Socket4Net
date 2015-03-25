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
using socket4net.Log;
using socket4net.Timer;
using System.Collections.Concurrent;

namespace socket4net.Service
{
    /// <summary>
    /// StaService is a component provides a producer/consumer
    /// pattern working queue, thus translate multiple
    /// threads model into single thread model.
    /// </summary>
    public class LogicService : ILogicService
    {
        private const int StopWatchDivider = 128;
        private Thread _workingThread;
        private bool _stopWorking;
        private BlockingCollection<IJob> _workingQueue;
        readonly System.Diagnostics.Stopwatch _watch = new System.Diagnostics.Stopwatch();
        private int _excutedJobsPerSec;
        private int _previousCalcTime;

        public TimerScheduler Scheduler { get; private set; }

        public void Start()
        {
            if (_workingQueue != null
                || _workingThread != null)
            {
                Logger.Instance.Fatal("STA being start more than once");
                return;
            }

            QueueCapacity = Capacity;
            Scheduler = new TimerScheduler(this);
            DoStartup();
        }

        public void Stop(bool joinWorker = true)
        {
            Scheduler.Dispose();

            if (_workingThread == null
                || _workingQueue == null)
            {
                Logger.Instance.Fatal("STA not yet been started");
                return;
            }

            _stopWorking = true;
            if (joinWorker)
            {
                _workingThread.Join();
            }
        }

        public void Perform(Action action)
        {
            Enqueue(action);
        }

        public void Perform<T>(Action<T> action, T param)
        {
            Enqueue(action, param);
        }

        /// <summary>
        /// Idle event, call by working thread
        /// every period.
        /// <remarks>generally the working thread
        /// call the event every period, but if it's too busy
        /// because the working item consumes too much time,
        /// the calling period may grater than the original period</remarks>
        /// </summary>
        public event Action Idle;
        
        /// <summary>
        /// Specify the capacity of the working item queue.
        /// <remarks>when the items count in working queue reach the capacity
        /// of the queue, all producer will block until there is one or more slot being free</remarks>
        /// </summary>
        public int QueueCapacity
        {
            get;
            private set;
        }
        /// <summary>
        /// Specify the working period of the working thread in milliseconds.
        /// That is, every period,the working thread loop back to the working
        ///  procedure's top. and, the Idle event is called.
        /// </summary>
        public int Period
        {
            get;
            set;
        }
        /// <summary>
        /// A time counter that count the work items consume how much time
        /// in one working thread's loop. It maybe grater than the working period,
        /// which indicates the work items consume too much time.
        /// </summary>
        public long WiElapsed
        {
            get;
            private set;
        }
        /// <summary>
        /// A time counter that count the Idle event callbacks consume how much
        /// time in one working thread's loop. This value should be less than period.
        /// </summary>
        public long IdleCallbackElapsed
        {
            get;
            private set;
        }
        /// <summary>
        /// Specify the work items count currently in working queue.
        /// </summary>
        public int Jobs
        {
            get { return _workingQueue.Count; }
        }

        public int ExcutedJobsPerSec
        {
            get { return _excutedJobsPerSec; }
            private set { _excutedJobsPerSec = value; }
        }

        public int Capacity { get; set; }
        int IService.Period
        {
            get { return Period; }
            set { Period = value; }
        }
        
        /// <summary>
        /// Get the elapsed milliseconds since the instance been constructed
        /// </summary>
        public long ElapsedMilliseconds
        {
            get { return _watch.ElapsedMilliseconds; }
        }

        /// <summary>
        /// External(e.g. the TCP socket thread) call this method to push
        /// a work item into the working queue. The work item must not
        /// be null.
        /// </summary>
        /// <param name="w">the work item object, must not be null</param>
        public void Enqueue(IJob w)
        {
            if (!_workingQueue.TryAdd(w, 0))
            {
                _workingQueue.Add(w);
            }
        }
        /// <summary>
        /// External(e.g. the TCP socket thread) call this method to push
        /// a work item into the working queue. 
        /// </summary>
        /// <param name="proc">the working procedure</param>
        /// <param name="param">additional parameter that passed to working procedure</param>
        public void Enqueue<T>(Action<T> proc, T param)
        {
            var w = new Job<T>(proc, param);
            Enqueue(w);
        }

        public void Enqueue(Action proc)
        {
            var job = new Job(proc);
            Enqueue(job);
        }

        /// <summary>
        /// do actually startup job
        /// </summary>
        private void DoStartup()
        {
            _workingQueue = new BlockingCollection<IJob>(new ConcurrentQueue<IJob>(), QueueCapacity);
            _workingThread = new Thread(WorkingProcedure) { IsBackground = true };

            // use background thread
            // see http://msdn.microsoft.com/en-us/library/h339syd0.aspx

            _workingThread.Start();
        }
        
        /// <summary>
        /// working thread's working procedure
        /// </summary>
        private void WorkingProcedure()
        {
            _watch.Start();

            int periodCounter;
            int handled;
            int tick;
            while (!_stopWorking)
            {
                periodCounter = StopWatchDivider;
                handled = 0;
                tick = Environment.TickCount;

                var t1 = _watch.ElapsedMilliseconds;

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
                                if ((_watch.ElapsedMilliseconds - t1) >= Period)
                                {
                                    break;
                                }
                                periodCounter = StopWatchDivider;
                            }
                        }
                        else
                            break;
                    }

#if DEBUG
                    catch (Exception ex)
                    {
                        throw;
#else
                    catch (Exception ex)
                    {   
                        OutputSTAFatalMessage(ex);
#endif
                    }

                }
                while ((Environment.TickCount - tick) < Period);

                WiElapsed = _watch.ElapsedMilliseconds - t1;

                if (Idle != null)
                {
                    var t2 = _watch.ElapsedMilliseconds;
                    try
                    {
                        Idle();
                    }
#if DEBUG
                    catch
                    {
                        throw;
#else
                    catch (Exception ex)
                    {
                        OutputSTAFatalMessage(ex);
#endif
                    }

                    IdleCallbackElapsed = _watch.ElapsedMilliseconds - t2;
                }
            }
        }

        private void CalcPerformance()
        {
            var delta = (Environment.TickCount - _previousCalcTime) / 1000.0f;
            if (delta < 1.0f)
                ++_excutedJobsPerSec;
            else
            {
                ExcutedJobsPerSec = (int)(_excutedJobsPerSec / delta);
                _previousCalcTime = Environment.TickCount;
                _excutedJobsPerSec = 0;
            }
        }

        private void OutputSTAFatalMessage(Exception ex)
        {
            try
            {
                int pid;
                using (var p = System.Diagnostics.Process.GetCurrentProcess())
                {
                    pid = p.Id;
                }

                var time = DateTime.Now.ToString("yyyy-MM-dd_hh.mm.ss");
                var logFileName = string.Format("STA({0})({1}.txt", pid, time);
                using (var ostream = new System.IO.StreamWriter(logFileName))
                {
                    ostream.WriteLine(ex.Message);
                    ostream.WriteLine("Stack trace:");
                    ostream.WriteLine(ex.StackTrace);
                    ostream.Close();
                }
            }
            catch
            {

            }
        }
    }
}
