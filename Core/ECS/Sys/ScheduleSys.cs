using System;
using System.Collections;
using System.Linq;
#if NET45
using System.Threading.Tasks;
#endif

namespace socket4net
{
    public class ScheduleSys : Obj
    {
        public const uint MillisecondsPerDay = 24 * 60 * 60 * 1000;
        public static ScheduleSys Instance { get; private set; }
        private Scheduler _scheduler;
        
        /// <summary>
        ///     产生一个在逻辑线程等待n毫秒的枚举器
        ///     用在协程中
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static IEnumerator WaitFor(uint n)
        {
            var meet = false;
            Instance.Invoke(() => { meet = true; }, n);

            while (!meet)
                yield return true;

            yield return false;
        }

        #region 协程

        public void StartCoroutine(Func<IEnumerator> fun)
        {
            var scheduler = GlobalVarPool.Instance.LogicService.CoroutineScheduler;
            scheduler.StartCoroutine(fun);
        }

        public void StartCoroutine(Func<object[], IEnumerator> fun, params object[] args)
        {
            var scheduler = GlobalVarPool.Instance.LogicService.CoroutineScheduler;
            scheduler.StartCoroutine(fun, args);
        }

        #endregion

        protected override void OnInit(ObjArg objArg)
        {
            base.OnInit(objArg);

            if (Instance != null)
                throw new Exception("ScheduleSys already instantiated!");
            Instance = this;

            var batchedOwner = Owner as IBatched;
            _scheduler = batchedOwner != null ?
                new BatchedScheduler(batchedOwner, Name) :
                new Scheduler(Name);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // 销毁定时器
            if (_scheduler != null)
                _scheduler.Destroy();
        }

        #region 定时调度

        /// <summary>
        ///     清理调度器
        /// </summary>
        public void ClearTimers()
        {
            if (_scheduler != null)
                _scheduler.Clear();
        }

        /// <summary>
        ///     延时delay，以period为周期重复执行action
        /// </summary>
        public void InvokeRepeating(Action action, uint delay, uint period)
        {
            if (_scheduler != null)
                _scheduler.InvokeRepeating(action, delay, period);
            else
                Logger.Instance.WarnFormat("Scheduler is null fo {0}", Name);
        }

        /// <summary>
        ///     延时delay，执行action
        /// </summary>
        /// <param name="action"></param>
        /// <param name="delay"></param>
        public void Invoke(Action action, uint delay)
        {
            if (_scheduler != null)
                _scheduler.Invoke(action, delay);
            else
                Logger.Instance.WarnFormat("Scheduler is null fo {0}", Name);
        }

        /// <summary>
        ///     在when时间点执行action
        /// </summary>
        /// <param name="action"></param>
        /// <param name="when"></param>
        public void Invoke(Action action, DateTime when)
        {
            if (_scheduler == null)
            {
                Logger.Instance.WarnFormat("Scheduler is null fo {0}", Name);
                return;
            }

            var delay = (when - DateTime.Now).TotalMilliseconds;
            if (delay < 0)
            {
                Logger.Instance.WarnFormat("调度请求时间已过期 {0}", Name);
                return;
            }

            _scheduler.Invoke(action, (uint)delay);
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
            if (_scheduler == null)
            {
                Logger.Instance.WarnFormat("Scheduler is null fo {0}", Name);
                return;
            }

            if (time.Days != 0)
            {
                Logger.Instance.WarnFormat("调度时间有误，{0}须小于一天 {1}", time, Name);
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
            if (_scheduler != null)
                _scheduler.CancelInvoke(action);
            else
                Logger.Instance.WarnFormat("Scheduler is null fo {0}", Name);
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
                    Logger.Instance.WarnFormat("定时调度时间点集错误 {0}", Name);
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
                Logger.Instance.WarnFormat("调度时间点指定错误 {0}", Name);
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
                if (_scheduler != null)
                    _scheduler.Invoke(() =>
                    {
                        action();
                        tcs.SetResult(true);
                    }, delay);
                else
                    Logger.Instance.WarnFormat("Scheduler is null fo {0}", Name);
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
