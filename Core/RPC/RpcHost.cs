using System;
using System.Collections.Generic;

namespace Core.RPC
{
    public abstract class RpcHost : IRpcHost
    {
        protected RpcHost(RpcSession session)
        {
            Session = session;
        }

        public RpcSession Session { get; private set; }

        protected Dictionary<short, Func<byte[], object>> RequestsHandlers = new Dictionary<short, Func<byte[], object>>();
        protected Dictionary<short, Func<byte[], bool>> NotifyHandlers = new Dictionary<short, Func<byte[], bool>>();

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

        protected void RegisterRequestHandler(short route, Func<byte[], object> handler)
        {
            RequestsHandlers.Add(route, handler);
        }

        protected void RegisterNotifyHandler(short route, Func<byte[], bool> handler)
        {
            NotifyHandlers.Add(route, handler);
        }

        protected abstract void RegisterRpcHandlers();
    }
}