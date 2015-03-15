
using System;
using Core.Timer;

namespace Core.Service
{
    /// <summary>
    /// 提供单线程服务
    /// </summary>
    public interface IService
    {
        int Jobs { get; }
        int Capacity { get; set; }
        int Period { get; set; }

        void Start();
        void Stop(bool joinWorker = true);

        void Perform(Action action);
        void Perform<T>(Action<T> action, T param);
    }


    /// <summary>
    /// 逻辑服务接口
    /// 定时器在逻辑服务调度
    /// </summary>
    public interface ILogicService : IService
    {
        event Action Idle;
        TimerScheduler Scheduler { get; }
    }
}
