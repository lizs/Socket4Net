using System;
using System.Collections.Generic;

namespace Core.RPC
{
    public interface IRpcHost : IDisposable
    {
        RpcSession Session { get; }
    }

    public abstract class RpcHost : IRpcHost
    {
        protected RpcHost(RpcSession session)
        {
            Session = session;
        }

        public RpcSession Session { get; private set; }

        protected Dictionary<RpcRoute, Func<byte[], object>> RequestsHandlers = new Dictionary<RpcRoute, Func<byte[], object>>();
        protected Dictionary<RpcRoute, Func<byte[], bool>> NotifyHandlers = new Dictionary<RpcRoute, Func<byte[], bool>>();

        public virtual void Boot()
        {
            RegisterRpcHandlers();

            foreach (var handler in NotifyHandlers)
                Session.Handlers.RegisterNotifyHandler(handler.Key, handler.Value);

            foreach (var handler in RequestsHandlers)
                Session.Handlers.RegisterRequestHandler(handler.Key, handler.Value);
        }

        public virtual void Dispose()
        {
            foreach (var route in NotifyHandlers.Keys)
                Session.Handlers.UnregisterNotifyHandlers(route);
            NotifyHandlers.Clear();

            foreach (var route in RequestsHandlers.Keys)
                Session.Handlers.UnregisterRequestHandlers(route);
            RequestsHandlers.Clear();

            Session = null;
        }

        protected void RegisterRequestHandler(RpcRoute route, Func<byte[], object> handler)
        {
            RequestsHandlers.Add(route, handler);
        }

        protected void RegisterNotifyHandler(RpcRoute route, Func<byte[], bool> handler)
        {
            NotifyHandlers.Add(route, handler);
        }

        protected abstract void RegisterRpcHandlers();
    }
}