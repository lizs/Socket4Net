using System;

namespace Core.RPC
{
    public interface IRpcHost : IDisposable
    {
        RpcSession Session { get; }


    }


    public interface IRpcHandler
    {
        bool CanHandle(short route);
    }

    public interface IRequestHandler
    {
        
    }

    public interface INotifyHandler
    {
        
    }
}
