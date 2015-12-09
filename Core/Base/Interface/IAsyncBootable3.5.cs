using System;

namespace Pi.Core
{
    public interface IAsyncBootable : IDisposable
    {
        void BootAsync();
        void Init();
    }
}