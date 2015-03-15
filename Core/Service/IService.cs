
using System;

namespace Core.Service
{
    public interface IService
    {
        int Jobs { get; }
        void Startup(int capacity, int period);
        void Shutdown(bool joinWorkingThread);
        void Perform(Action action);
        void Perform<T>(Action<T> action, T param);
    }
}
