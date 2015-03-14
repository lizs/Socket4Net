using System;
using System.Collections.Generic;

namespace Core.RPC
{
    /// <summary>
    /// Rpc处理器
    /// </summary>
    public class RpcHandlers : IDisposable
    {
        private readonly Dictionary<RpcRoute, Func<byte[], object>> _requestHandlers =
            new Dictionary<RpcRoute, Func<byte[], object>>();
        private readonly Dictionary<RpcRoute, Func<byte[], bool>> _notifyHandlers =
            new Dictionary<RpcRoute, Func<byte[], bool>>();

        public bool RegisterRequestHandler(RpcRoute route, Func<byte[], object> handler)
        {
            if (_requestHandlers.ContainsKey(route)) return false;
            _requestHandlers.Add(route, handler);
            return true;
        }
        
        public bool RegisterNotifyHandler(RpcRoute route, Func<byte[], bool> handler)
        {
            if (_notifyHandlers.ContainsKey(route)) return false;
            _notifyHandlers.Add(route, handler);
            return true;
        }

        public void UnregisterRequestHandlers(RpcRoute route)
        {
            if (!_requestHandlers.ContainsKey(route)) return;
            _requestHandlers.Remove(route);
        }

        public void UnregisterNotifyHandlers(RpcRoute route)
        {
            if (!_notifyHandlers.ContainsKey(route)) return;
            _notifyHandlers.Remove(route);
        }

        public void Dispose()
        {
            _requestHandlers.Clear();
            _notifyHandlers.Clear();
        }

        public object HandleRequest(RpcRoute route, byte[] param)
        {
            var handler = _requestHandlers.ContainsKey(route) ? _requestHandlers[route] : null;
            return handler == null ? null : _requestHandlers[route](param);
        }

        public bool HandleNotify(RpcRoute route, byte[] param)
        {
            var handler = _notifyHandlers.ContainsKey(route) ? _notifyHandlers[route] : null;
            if(handler == null) return false;
            _notifyHandlers[route](param);
            return true;
        }
    }
}
