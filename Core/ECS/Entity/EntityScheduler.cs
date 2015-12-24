using System;
using System.Collections;
using System.Threading.Tasks;

namespace socket4net
{
    /// <summary>
    ///     E(ECS)
    ///     µ÷¶È²Ù×÷
    /// </summary>
    public sealed partial class Entity
    {
        public IEnumerator WaitFor(uint ms)
        {
            return ScheduleSys.WaitFor(ms);
        }

        public void StartCoroutine(Func<IEnumerator> fun)
        {
            ScheduleSys.Instance.StartCoroutine(fun);
        }

        public void StartCoroutine(Func<object[], IEnumerator> fun, params object[] args)
        {
            ScheduleSys.Instance.StartCoroutine(fun, args);
        }

        public void ClearTimers()
        {
            ScheduleSys.Instance.ClearTimers();
        }

        public void InvokeRepeating(Action action, uint delay, uint period)
        {
            ScheduleSys.Instance.InvokeRepeating(action, delay, period);
        }

        public void Invoke(Action action, uint delay)
        {
            ScheduleSys.Instance.Invoke(action, delay);
        }

        public void Invoke(Action action, DateTime when)
        {
            ScheduleSys.Instance.Invoke(action, when);
        }

        public void Invoke(Action action, int hour, int min, int s)
        {
            ScheduleSys.Instance.Invoke(action, hour, min, s);
        }

        public void Invoke(Action action, TimeSpan time)
        {
            ScheduleSys.Instance.Invoke(action, time);
        }

        public void CancelInvoke(Action action)
        {
            ScheduleSys.Instance.CancelInvoke(action);
        }

#if NET45
        public Task<bool> InvokeAsync(Action action, params TimeSpan[] times)
        {
            return ScheduleSys.Instance.InvokeAsync(action, times);
        }

        public Task<bool> InvokeAsync(Action action, params DateTime[] whens)
        {
            return ScheduleSys.Instance.InvokeAsync(action, whens);
        }

        public Task<bool> InvokeAsync(Action action, TimeSpan time)
        {
            return ScheduleSys.Instance.InvokeAsync(action, time);
        }

        public Task<bool> InvokeAsync(Action action, DateTime when)
        {
            return ScheduleSys.Instance.InvokeAsync(action, when);
        }

        public Task<bool> InvokeAsync(Action action, uint delay)
        {
            return ScheduleSys.Instance.InvokeAsync(action, delay);
        }
#endif
    }
}