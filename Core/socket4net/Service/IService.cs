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
    public class ServiceArg : ObjArg
    {
        public ServiceArg(IObj owner, int capacity, int period) : base(owner)
        {
            Capacity = capacity;
            Period = period;
        }

        public int Capacity { get; private set; }
        public int Period { get; private set; }
    }

    /// <summary>
    /// 提供单线程服务
    /// </summary>
    public interface IService : IObj
    {
        int Capacity { get; }
        int Period { get; }

        void Perform(Action action);
        void Perform<T>(Action<T> action, T param);

        // performance
        int Jobs { get; }
        int ExcutedJobsPerSec { get; }
    }

    /// <summary>
    /// 网络服务接口
    /// </summary>
    public interface INetService : IService
    {
        // performance 4 net
        void OnReadCompleted(int len, ushort cnt);
        void OnWriteCompleted(int len);

        int ReadBytesPerSec { get; }
        int WriteBytesPerSec { get; }
        int ReadPackagesPerSec { get; }
        int WritePackagesPerSec { get; }
    }

    /// <summary>
    /// 逻辑服务接口
    /// 定时器在逻辑服务调度
    /// </summary>
    public interface ILogicService : IService
    {
        event Action Idle;
        TimerScheduler Scheduler { get; }
        CoroutineScheduler CoroutineScheduler { get; }
        long ElapsedMilliseconds { get; }
    }
}
