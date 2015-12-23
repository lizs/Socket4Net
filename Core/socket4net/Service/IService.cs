
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
