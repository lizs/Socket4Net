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
using System.Collections;
using System.Linq;
#if NET45
using System.Threading.Tasks;
#endif

namespace socket4net
{
    /// <summary>
    ///     对象基类
    /// </summary>
    public abstract partial class Obj
    {
        /// <summary>
        ///     
        /// </summary>
        public const uint MillisecondsPerDay = 24 * 60 * 60 * 1000;
        private Scheduler _cheduler;

        #region 协程

        IEnumerator IObj.WaitFor(uint n)
        {
            var meet = false;
            (this as IObj).Invoke(() => { meet = true; }, n);

            while (!meet)
                yield return true;

            yield return false;
        }
        
        void IObj.StopCoroutine(Coroutine coroutine)
        {
            var scheduler = GlobalVarPool.Ins.Service.CoroutineScheduler;
            scheduler.InternalStopCoroutine(coroutine);
        }

        Coroutine IObj.StartCoroutine(Func<IEnumerator> fun)
        {
            var scheduler = GlobalVarPool.Ins.Service.CoroutineScheduler;
            return scheduler.InternalStartCoroutine(fun);
        }

        Coroutine IObj.StartCoroutine(Func<object[], IEnumerator> fun, params object[] args)
        {
            var scheduler = GlobalVarPool.Ins.Service.CoroutineScheduler;
            return scheduler.InternalStartCoroutine(fun, args);
        }

        #endregion

        /// <summary>
        ///    internal called when an Obj is initialized
        /// </summary>
        /// <param name="arg"></param>
        protected virtual void OnInit(ObjArg arg)
        {
            Owner = arg.Owner;

            var flushableAncestor = (this as IObj).GetAncestor<IFlushable>();
            _cheduler = flushableAncestor != null ? new BatchedScheduler(flushableAncestor) : new Scheduler();
        }

        /// <summary>
        ///    internal called when an Obj is to be destroyed
        /// </summary>
        protected virtual void OnDestroy()
        {
            // 销毁定时器
            (_cheduler as IObj)?.Destroy();
        }

        #region 定时调度

        /// <summary>
        ///     clear timers attached on this obj
        /// </summary>
        public void ClearTimers()
        {
            _cheduler?.Clear();
        }

        /// <summary>
        ///     Excute 'action' after 'delay' ms for every 'period' ms
        /// </summary>
        public void InvokeRepeating(Action action, uint delay, uint period)
        {
            if (_cheduler != null)
                _cheduler.InvokeRepeatingImp(action, delay, period);
            else
                Logger.Ins.Warn("Scheduler is null fo {0}", Name);
        }

        /// <summary>
        ///     Excute 'action' after 'delay' ms
        /// </summary>
        /// <param name="action"></param>
        /// <param name="delay"></param>
        public void Invoke(Action action, uint delay)
        {
            if (_cheduler != null)
                _cheduler.InvokeImp(action, delay);
            else
                Logger.Ins.Warn("Scheduler is null fo {0}", Name);
        }

        /// <summary>
        ///     Excute 'action' when 'when'
        /// </summary>
        /// <param name="action"></param>
        /// <param name="when"></param>
        public void Invoke(Action action, DateTime when)
        {
            if (_cheduler == null)
            {
                Logger.Ins.Warn("Scheduler is null for {0}", Name);
                return;
            }

            var delay = (when - DateTime.Now).TotalMilliseconds;
            if (delay < 0)
            {
                Logger.Ins.Warn("调度请求时间已过期 {0}", Name);
                return;
            }

            _cheduler.InvokeImp(action, (uint) delay);
        }

        /// <summary>
        ///     Excute 'action' every 'hour:min:s'
        /// </summary>
        /// <param name="action"></param>
        /// <param name="hour"></param>
        /// <param name="min"></param>
        /// <param name="s"></param>
        public void Invoke(Action action, int hour, int min, int s)
        {
            (this as IObj).Invoke(action, new TimeSpan(0, hour, min, s));
        }

        /// <summary>
        ///     Excute 'action' everyday's 'time' clock
        /// </summary>
        /// <param name="action"></param>
        /// <param name="time"></param>
        public void Invoke(Action action, TimeSpan time)
        {
            if (_cheduler == null)
            {
                Logger.Ins.Warn("Scheduler is null fo {0}", Name);
                return;
            }

            if (time.Days != 0)
            {
                Logger.Ins.Warn("调度时间有误，{0}须小于一天 {1}", time, Name);
                return;
            }

            var now = DateTime.Now;
            var actionTime = now.Date.AddHours(time.Hours).AddMinutes(time.Minutes).AddSeconds(time.Seconds);
            if (actionTime < now)
                actionTime = actionTime.AddDays(1);

            var delay = (uint)(actionTime - now).TotalMilliseconds;
            (this as IObj).InvokeRepeating(action, delay, MillisecondsPerDay);
        }
        
        /// <summary>
        ///     取消action的调度
        /// </summary>
        /// <param name="action"></param>
        public void CancelInvoke(Action action)
        {
            if (_cheduler != null)
                _cheduler.CancelInvokeImp(action);
            else
                Logger.Ins.Warn("Scheduler is null fo {0}", Name);
        }
        #endregion

#if NET45

        #region asyn调度

        /// <summary>
        ///     每日的times时间点执行action
        /// </summary>
        /// 
        public async Task<bool> InvokeAsync(Action action, params TimeSpan[] times)
        {
            while (true)
            {
                if (times.IsNullOrEmpty())
                {
                    Logger.Ins.Warn("定时调度时间点集错误 {0}", Name);
                    return false;
                }

                var now = DateTime.Now;
                var nowTs = new TimeSpan(0, now.Hour, now.Minute, now.Second);

                for (var i = 0; i < times.Length; i++)
                {
                    if (times[i] < nowTs)
                        times[i] = times[i].Add(new TimeSpan(1));
                }

                var sorted = times.ToList();
                sorted.Sort();

                foreach (var ts in sorted)
                {
                    await InvokeAsync(action, ts);
                }

                for (var i = 0; i < times.Length; i++)
                {
                    times[i] = times[i].Add(new TimeSpan(1));
                }
            }
        }

        /// <summary>
        ///     在whens指定的时间点执行action
        /// </summary>
        public async Task<bool> InvokeAsync(Action action, params DateTime[] whens)
        {
            if (whens.IsNullOrEmpty())
            {
                Logger.Ins.Warn("调度时间点指定错误 {0}", Name);
                return false;
            }

            var sorted = whens.ToList();
            sorted.RemoveAll(x => x < DateTime.Now);
            sorted.Sort();

            foreach (var t in sorted)
            {
                await InvokeAsync(action, t);
            }

            return true;
        }

        public async Task<bool> InvokeAsync(Action action, TimeSpan time)
        {
            var now = DateTime.Now;
            var nowTs = new TimeSpan(0, now.Hour, now.Minute, now.Second);
            if (time < nowTs)
                time = time.Add(new TimeSpan(1));

            var delay = (uint)(time.TotalMilliseconds - nowTs.TotalMilliseconds);
            return await InvokeAsync(action, delay);
        }

        public async Task<bool> InvokeAsync(Action action, DateTime when)
        {
            var now = DateTime.Now;
            if (when < now) return false;

            var delay = (uint)((when - now).TotalMilliseconds);
            return await InvokeAsync(action, delay);
        }

        public async Task<bool> InvokeAsync(Action action, uint delay)
        {
            var tcs = new TaskCompletionSource<bool>();

            try
            {
                if (_cheduler != null)
                    _cheduler.InvokeImp(() =>
                    {
                        action();
                        tcs.SetResult(true);
                    }, delay);
                else
                    Logger.Ins.Warn("Scheduler is null fo {0}", Name);
            }
            catch (Exception e)
            {
                tcs.SetException(e);
            }

            return await tcs.Task;
        }

        #endregion
#endif
    }
}