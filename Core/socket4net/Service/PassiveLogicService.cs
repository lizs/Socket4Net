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

        /// <summary>
        ///     Invoked when obj started
        /// </summary>
        protected override void OnStart()
        {
            base.OnStart();

            StopWorking = false;
            Watch.Start();
            Logger.Ins.Debug("Passive logic service started!");
        }

        /// <summary>
        ///    internal called when an Obj is to be destroyed
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();

            StopWorking = true;
            Watch.Stop();
            Logger.Ins.Debug("Logic service stopped!");
        }

        /// <summary>
        ///     Specify the work items count currently in working queue.
        /// </summary>
        public override int Jobs => _jobs.Count;

        internal override void Enqueue(IJob w)
        {
            _jobs.Enqueue(w);
        }

        /// <summary>
        ///     passive logic service need update timer manual
        /// </summary>
        public void UpdateTimer()
        {
            try
            {
                Idle?.Invoke();
            }
            catch (Exception e)
            {
                Logger.Ins.Exception("PassiveLogicService", e);
            }
        }

        /// <summary>
        ///     passive logic service need update queue manual
        /// </summary>
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
                    Logger.Ins.Exception("PassiveLogicService", e);
                }

                return;
            }

            try
            {
                using (var watch = new AutoWatch(""))
                {
                    IJob job;
                    while (watch.ElapsedMilliseconds < 30 && _jobs.TryDequeue(out job))
                    {
                        job.Do();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Ins.Exception("PassiveLogicService", e);
            }
        }

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
        public override event Action Idle;
    }
}
