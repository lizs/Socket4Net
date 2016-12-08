#region MIT

//  /*The MIT License (MIT)
// 
//  Copyright 2016 lizs lizs4ever@163.com
//  
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//  
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
//  
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
//   * */

#endregion

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace socket4net
{
    /// <summary>
    ///     service
    ///     1、provide single thread excution environment
    ///     2、schedule timer and coroutine
    /// </summary>
    public sealed class Service : Obj
    {
        private const int StopWatchDivider = 128;
        private bool _stopWorking;
        private readonly Stopwatch _watch = new Stopwatch();

        /// <summary>
        ///     underline working thread
        /// </summary>
        private Thread _workingThread;

        /// <summary>
        ///     working queue
        /// </summary>
        private BlockingCollection<IJob> _workingQueue;

        /// <summary>
        ///     timer scheduler
        /// </summary>
        public TimerScheduler Scheduler { get; private set; }

        /// <summary>
        ///     coroutine scheduler
        /// </summary>
        public CoroutineScheduler CoroutineScheduler { get; private set; }

        /// <summary>
        ///     Idle event, call by working thread
        ///     every period.
        ///     <remarks>
        ///         generally the working thread
        ///         call the event every period, but if it's too busy
        ///         because the working item consumes too much time,
        ///         the calling period may grater than the original period
        ///     </remarks>
        /// </summary>
        public event Action Idle;

        /// <summary>
        ///     Specify the capacity of the working item queue.
        ///     <remarks>
        ///         when the items count in working queue reach the capacity
        ///         of the queue, all producer will block until there is one or more slot being free
        ///     </remarks>
        /// </summary>
        public int QueueCapacity { get; set; } = 1000000;

        /// <summary>
        ///     Specify the working period of the working thread in milliseconds.
        ///     That is, every period,the working thread loop back to the working
        ///     procedure's top. and, the Idle event is called.
        /// </summary>
        public int Period { get; set; } = 10;

        /// <summary>
        ///     A time counter that count the work items consume how much time
        ///     in one working thread's loop. It maybe grater than the working period,
        ///     which indicates the work items consume too much time.
        /// </summary>
        public long WiElapsed { get; private set; }

        /// <summary>
        ///     A time counter that count the Idle event callbacks consume how much
        ///     time in one working thread's loop. This value should be less than period.
        /// </summary>
        public long IdleCallbackElapsed { get; private set; }

        /// <summary>
        ///     Get the elapsed milliseconds since the instance been constructed
        /// </summary>
        public long ElapsedMilliseconds => _watch.ElapsedMilliseconds;

        /// <summary>
        ///     excute
        /// </summary>
        /// <param name="action"></param>
        public void Perform(Action action)
        {
            var w = new Job(action);
            if (!_workingQueue.TryAdd(w))
            {
                Logger.Ins.Error($"Working queue of capacity [{QueueCapacity}] is full, action discarded!");
            }
        }

        /// <summary>
        ///     excute action produced by other threads
        /// </summary>
        /// <param name="action"></param>
        /// <param name="param"></param>
        public void Perform<T>(Action<T> action, T param)
        {
            var w = new Job<T>(action, param);
            if (!_workingQueue.TryAdd(w))
            {
                Logger.Ins.Error($"Working queue of capacity [{QueueCapacity}] is full, action discarded!");
            }
        }

        /// <summary>
        ///     异步执行
        /// </summary>
        /// <param name="action"></param>
        public void Perform(Func<Task<RpcResult>> action)
        {
            var w = new Job(() =>
            {
                TaskHelper.ExcuteTask(action);
            });
            
            if (!_workingQueue.TryAdd(w))
            {
                Logger.Ins.Error($"Working queue of capacity [{QueueCapacity}] is full, action discarded!");
            }
        }

        /// <summary>
        ///     Invoked when obj started
        /// </summary>
        protected override void OnStart()
        {
            base.OnStart();

            Scheduler = new TimerScheduler(this);
            CoroutineScheduler = Create<CoroutineScheduler>(new ObjArg(this), true);

            if (_workingQueue != null
                || _workingThread != null)
            {
                Logger.Ins.Fatal("Service being start more than once");
                return;
            }

            _workingQueue = new BlockingCollection<IJob>(QueueCapacity);
            _workingThread = new Thread(WorkingProcedure) { Name = "Service", IsBackground = true };
            // use background thread
            // see http://msdn.microsoft.com/en-us/library/h339syd0.aspx

            _workingThread.Start();
            Logger.Ins.Debug("Auto logic service started!");
        }

        /// <summary>
        ///     internal called when an Obj is to be destroyed
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();

            Scheduler.Dispose();
            CoroutineScheduler.Destroy();

            if (_workingThread == null
                || _workingQueue == null)
            {
                Logger.Ins.Fatal("Service not yet been started");
                return;
            }

            _stopWorking = true;
            _workingThread.Join();
            Logger.Ins.Debug("Service stopped!");
        }

        /// <summary>
        ///     Specify the work items count currently in working queue.
        /// </summary>
        public int Jobs => _workingQueue.Count;
        
        /// <summary>
        ///     working thread's working procedure
        /// </summary>
        private void WorkingProcedure()
        {
            _watch.Start();

            while (!_stopWorking)
            {
                var periodCounter = StopWatchDivider;
                //var tick = Environment.TickCount;

                var t1 = _watch.ElapsedMilliseconds;

                try
                {
                    IJob item;
                    while (_workingQueue.TryTake(out item, Period))
                    {
                        item.Do();
                        PerformanceMonitor.Ins.RecordJob();

                        //                            periodCounter--;
                        //                            if (periodCounter > 0) continue;
                        //
                        if (_watch.ElapsedMilliseconds - t1 >= Period)
                        {
                            break;
                        }
                        //                            periodCounter = StopWatchDivider;

                    }
//                        else
//                            break;

                }
                catch (Exception ex)
                {
                    Logger.Ins.Fatal("{0} : {1}", ex.Message, ex.StackTrace);
                }

                WiElapsed = _watch.ElapsedMilliseconds - t1;
                if (Idle == null) continue;

                var t2 = _watch.ElapsedMilliseconds;
                try
                {
                    Idle();
                }
                catch (Exception ex)
                {
                    Logger.Ins.Fatal($"{ex.Message} : {ex.StackTrace}");
                }

                IdleCallbackElapsed = _watch.ElapsedMilliseconds - t2;
            }


            IJob leftItem;
            while (_workingQueue.TryTake(out leftItem, Period*10))
            {
                leftItem.Do();
            }

            _watch.Stop();
        }
    }
}