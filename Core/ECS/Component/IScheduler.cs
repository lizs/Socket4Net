using System;
using System.Collections;
using System.Threading.Tasks;

namespace socket4net
{
    public interface IScheduler
    {
        /// <summary>
        ///     产生一个在当前线程等待ms毫秒的枚举器
        ///     用在协程中
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        IEnumerator WaitFor(uint ms);

        void StartCoroutine(Func<IEnumerator> fun);
        void StartCoroutine(Func<object[], IEnumerator> fun, params object[] args);

        /// <summary>
        ///     清理调度器
        /// </summary>
        void ClearTimers();

        /// <summary>
        ///     延时delay，以period为周期重复执行action
        /// </summary>
        void InvokeRepeating(Action action, uint delay, uint period);

        /// <summary>
        ///     延时delay，执行action
        /// </summary>
        /// <param name="action"></param>
        /// <param name="delay"></param>
        void Invoke(Action action, uint delay);

        /// <summary>
        ///     在when时间点执行action
        /// </summary>
        /// <param name="action"></param>
        /// <param name="when"></param>
        void Invoke(Action action, DateTime when);

        /// <summary>
        ///     每天 hour:min:s 执行action
        ///     如：每天20:15执行action，此时 hour == 20 min == 15 s == 0
        /// </summary>
        /// <param name="action"></param>
        /// <param name="hour"></param>
        /// <param name="min"></param>
        /// <param name="s"></param>
        void Invoke(Action action, int hour, int min, int s);

        /// <summary>
        ///     每天 time 执行action
        ///     注：time并非间隔
        /// </summary>
        /// <param name="action"></param>
        /// <param name="time"></param>
        void Invoke(Action action, TimeSpan time);

        /// <summary>
        ///     取消action的调度
        /// </summary>
        /// <param name="action"></param>
        void CancelInvoke(Action action);

        /// <summary>
        ///     每日的times时间点执行action
        /// </summary>
        /// 
        Task<bool> InvokeAsync(Action action, params TimeSpan[] times);

        /// <summary>
        ///     在whens指定的时间点执行action
        /// </summary>
        Task<bool> InvokeAsync(Action action, params DateTime[] whens);
        Task<bool> InvokeAsync(Action action, TimeSpan time);
        Task<bool> InvokeAsync(Action action, DateTime when);
        Task<bool> InvokeAsync(Action action, uint delay);
    }
}