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
        public const uint MillisecondsPerDay = 24 * 60 * 60 * 1000;
        private Scheduler _cheduler;

        #region 协程

        /// <summary>
        ///     产生一个在逻辑线程等待n毫秒的枚举器
        ///     用在协程中
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public IEnumerator WaitFor(uint n)
        {
            var meet = false;
            Invoke(() => { meet = true; }, n);

            while (!meet)
                yield return true;

            yield return false;
        }

        public void StopCoroutine(Coroutine coroutine)
        {
            var scheduler = GlobalVarPool.Ins.LogicService.CoroutineScheduler;
            scheduler.InternalStopCoroutine(coroutine);
        }

        public Coroutine StartCoroutine(Func<IEnumerator> fun)
        {
            var scheduler = GlobalVarPool.Ins.LogicService.CoroutineScheduler;
            return scheduler.InternalStartCoroutine(fun);
        }

        public Coroutine StartCoroutine(Func<object[], IEnumerator> fun, params object[] args)
        {
            var scheduler = GlobalVarPool.Ins.LogicService.CoroutineScheduler;
            return scheduler.InternalStartCoroutine(fun, args);
        }

        #endregion

        protected virtual void OnInit(ObjArg arg)
        {
            Owner = arg.Owner;

            var flushableAncestor = GetAncestor<IFlushable>();
            _cheduler = flushableAncestor != null ? new BatchedScheduler(flushableAncestor) : new Scheduler();
        }

        protected virtual void OnDestroy()
        {
            // 销毁定时器
            if (_cheduler != null)
                _cheduler.Destroy();
        }

        #region 定时调度

        /// <summary>
        ///     清理调度器
        /// </summary>
        public void ClearTimers()
        {
            if (_cheduler != null)
                _cheduler.Clear();
        }

        /// <summary>
        ///     延时delay，以period为周期重复执行action
        /// </summary>
        public void InvokeRepeating(Action action, uint delay, uint period)
        {
            if (_cheduler != null)
                _cheduler.InvokeRepeating(action, delay, period);
            else
                Logger.Ins.Warn("Scheduler is null fo {0}", Name);
        }

        /// <summary>
        ///     延时delay，执行action
        /// </summary>
        /// <param name="action"></param>
        /// <param name="delay"></param>
        public void Invoke(Action action, uint delay)
        {
            if (_cheduler != null)
                _cheduler.Invoke(action, delay);
            else
                Logger.Ins.Warn("Scheduler is null fo {0}", Name);
        }

        /// <summary>
        ///     在when时间点执行action
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

            _cheduler.Invoke(action, (uint)delay);
        }

        /// <summary>
        ///     每天 hour:min:s 执行action
        ///     如：每天20:15执行action，此时 hour == 20 min == 15 s == 0
        /// </summary>
        /// <param name="action"></param>
        /// <param name="hour"></param>
        /// <param name="min"></param>
        /// <param name="s"></param>
        public void Invoke(Action action, int hour, int min, int s)
        {
            Invoke(action, new TimeSpan(0, hour, min, s));
        }

        /// <summary>
        ///     每天 time 执行action
        ///     注：time并非间隔
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
            InvokeRepeating(action, delay, MillisecondsPerDay);
        }

        /// <summary>
        ///     取消action的调度
        /// </summary>
        /// <param name="action"></param>
        public void CancelInvoke(Action action)
        {
            if (_cheduler != null)
                _cheduler.CancelInvoke(action);
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
                    _cheduler.Invoke(() =>
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