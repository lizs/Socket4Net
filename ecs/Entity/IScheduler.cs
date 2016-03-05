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
#if NET45
using System.Threading.Tasks;
#endif

namespace ecs
{
//    public interface IScheduler
//    {
//        /// <summary>
//        ///     产生一个在当前线程等待ms毫秒的枚举器
//        ///     用在协程中
//        /// </summary>
//        /// <param name="ms"></param>
//        /// <returns></returns>
//        IEnumerator WaitFor(uint ms);

//        void StartCoroutine(Func<IEnumerator> fun);
//        void StartCoroutine(Func<object[], IEnumerator> fun, params object[] args);

//        /// <summary>
//        ///     清理调度器
//        /// </summary>
//        void ClearTimers();

//        /// <summary>
//        ///     延时delay，以period为周期重复执行action
//        /// </summary>
//        void InvokeRepeating(Action action, uint delay, uint period);

//        /// <summary>
//        ///     延时delay，执行action
//        /// </summary>
//        /// <param name="action"></param>
//        /// <param name="delay"></param>
//        void Invoke(Action action, uint delay);

//        /// <summary>
//        ///     在when时间点执行action
//        /// </summary>
//        /// <param name="action"></param>
//        /// <param name="when"></param>
//        void Invoke(Action action, DateTime when);

//        /// <summary>
//        ///     每天 hour:min:s 执行action
//        ///     如：每天20:15执行action，此时 hour == 20 min == 15 s == 0
//        /// </summary>
//        /// <param name="action"></param>
//        /// <param name="hour"></param>
//        /// <param name="min"></param>
//        /// <param name="s"></param>
//        void Invoke(Action action, int hour, int min, int s);

//        /// <summary>
//        ///     每天 time 执行action
//        ///     注：time并非间隔
//        /// </summary>
//        /// <param name="action"></param>
//        /// <param name="time"></param>
//        void Invoke(Action action, TimeSpan time);

//        /// <summary>
//        ///     取消action的调度
//        /// </summary>
//        /// <param name="action"></param>
//        void CancelInvoke(Action action);

//#if NET45
//        /// <summary>
//        ///     每日的times时间点执行action
//        /// </summary>
//        /// 
//        Task<bool> InvokeAsync(Action action, params TimeSpan[] times);

//        /// <summary>
//        ///     在whens指定的时间点执行action
//        /// </summary>
//        Task<bool> InvokeAsync(Action action, params DateTime[] whens);
//        Task<bool> InvokeAsync(Action action, TimeSpan time);
//        Task<bool> InvokeAsync(Action action, DateTime when);
//        Task<bool> InvokeAsync(Action action, uint delay);
//#endif
//    }
}