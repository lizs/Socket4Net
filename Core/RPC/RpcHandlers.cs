using System;
using System.Collections.Generic;

namespace Core.RPC
{
    /// <summary>
    /// Rpc处理器
    /// </summary>
    public class RpcHandlers : IDisposable
    {
        private readonly Dictionary<short, Func<byte[], object>> _requestHandlers =
            new Dictionary<short, Func<byte[], object>>();
        private readonly Dictionary<short, Func<byte[], bool>> _notifyHandlers =
            new Dictionary<short, Func<byte[], bool>>();

        public bool RegisterRequestHandler(short route, Func<byte[], object> handler)
        {
            if (_requestHandlers.ContainsKey(route)) return false;
            _requestHandlers.Add(route, handler);
            return true;
        }

        public bool RegisterNotifyHandler(short route, Func<byte[], bool> handler)
        {
            if (_notifyHandlers.ContainsKey(route)) return false;
            _notifyHandlers.Add(route, handler);
            return true;
        }

        public void UnregisterRequestHandlers(short route)
        {
            if (!_requestHandlers.ContainsKey(route)) return;
            _requestHandlers.Remove(route);
        }

        public void UnregisterNotifyHandlers(short route)
        {
            if (!_notifyHandlers.ContainsKey(route)) return;
            _notifyHandlers.Remove(route);
        }

        public void Dispose()
        {
            _requestHandlers.Clear();
            _notifyHandlers.Clear();
        }

        public object HandleRequest(short route, byte[] param)
        {
            var handler = _requestHandlers.ContainsKey(route) ? _requestHandlers[route] : null;
            return handler == null ? null : _requestHandlers[route](param);
        }

        public bool HandleNotify(short route, byte[] param)
        {
            var handler = _notifyHandlers.ContainsKey(route) ? _notifyHandlers[route] : null;
            if(handler == null) return false;
            _notifyHandlers[route](param);
            return true;
        }
    }
}
