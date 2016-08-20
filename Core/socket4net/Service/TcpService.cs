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
using System.Threading;
#if NET45
using System.Collections.Concurrent;
#endif

namespace socket4net
{
    /// <summary>
    /// 网络服务线程（网络数据读写）
    /// </summary>
    public class TcpService : Obj, ITcpService
    {
        private BlockingCollection<IJob> _jobs;
        private Thread _wokerThread;
        private bool _stopping;

        /// <summary>
        ///     get processing jobs count
        /// </summary>
        public int Jobs => _jobs.Count;
        
        /// <summary>
        ///     Jobs queued upper bound
        /// </summary>
        public int Capacity { get; set; }

        /// <summary>
        ///     Period for waiting concurrent collection's item take
        /// </summary>
        public int Period { get; set; } = 10;


        /// <summary>
        ///    internal called when an Obj is initialized
        /// </summary>
        /// <param name="arg"></param>
        protected override void OnInit(ObjArg arg)
        {
            base.OnInit(arg);

            Capacity = arg.As<ServiceArg>().Capacity;
            Period = arg.As<ServiceArg>().Period;
        }

        /// <summary>
        ///     Invoked when obj started
        /// </summary>
        protected override void OnStart()
        {
            base.OnStart();

            _jobs = new BlockingCollection<IJob>(Capacity);
            _wokerThread = new Thread(WorkingProcedure) {Name = "TcpService"};
            _wokerThread.Start();
            Logger.Ins.Debug("Net service started!");
        }

        /// <summary>
        ///    internal called when an Obj is to be destroyed
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();
            _stopping = true;
            _wokerThread.Join();
            Logger.Ins.Debug("Net service stopped!");
        }

        /// <summary>
        ///     Send action to be excuted in current service
        /// </summary>
        /// <param name="action"></param>
        public void Perform(Action action)
        {
            Enqueue(action);
        }

        /// <summary>
        ///     Send action to be excuted in current service
        /// </summary>
        /// <param name="action"></param>
        /// <param name="param"></param>
        /// <typeparam name="T"></typeparam>
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
                try
                {
                    IJob job;
                    if (_jobs.TryTake(out job, Period))
                    {
                        job.Do();
                        PerformanceMonitor.Ins.RecordJob();
                    }
                }
                catch (Exception e)
                {
                    Logger.Ins.Error("{0} : {1}", e.Message, e.StackTrace);
                }
            }

            try
            {
                IJob leftJob;
                while (_jobs.TryTake(out leftJob, Period * 10))
                {
                    leftJob.Do();
                }
            }
            catch (Exception e)
            {
                Logger.Ins.Error("{0} : {1}", e.Message, e.StackTrace);
            }
        }
    }
}
