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

namespace socket4net
{
    /// <summary>
    ///     Obj argument for service
    /// </summary>
    public class ServiceArg : ObjArg
    {
        public ServiceArg(IObj owner, int capacity, int period) : base(owner)
        {
            Capacity = capacity;
            Period = period;
        }

        /// <summary>
        /// 
        /// </summary>
        public int Capacity { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public int Period { get; private set; }
    }

    /// <summary>
    ///  STA service
    /// </summary>
    public interface IService : IObj
    {
        /// <summary>
        ///     Jobs queued upper bound
        /// </summary>
        int Capacity { get; }

        /// <summary>
        ///     Period for waiting concurrent collection's item take
        /// </summary>
        int Period { get; }

        /// <summary>
        ///     Send action to be excuted in current service
        /// </summary>
        /// <param name="action"></param>
        void Perform(Action action);

        /// <summary>
        ///     Send action to be excuted in current service
        /// </summary>
        /// <param name="action"></param>
        /// <param name="param"></param>
        /// <typeparam name="T"></typeparam>
        void Perform<T>(Action<T> action, T param);
    }

    /// <summary>
    ///     net work service
    /// </summary>
    public interface ITcpService : IService
    {
    }

    /// <summary>
    ///     Logic service
    ///     Timer schedule is running on Logic service
    /// </summary>
    public interface ILogicService : IService
    {
        /// <summary>
        ///     Idle event
        ///     Raised when no more jobs in a frame
        /// </summary>
        event Action Idle;

        /// <summary>
        ///     Timer scheduler
        /// </summary>
        TimerScheduler Scheduler { get; }

        /// <summary>
        ///     Coroutine scheduler
        /// </summary>
        CoroutineScheduler CoroutineScheduler { get; }

        /// <summary>
        ///     Elapsed ms since process started
        /// </summary>
        long ElapsedMilliseconds { get; }
    }
}
