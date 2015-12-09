using System;
#if NET45
using System.Threading.Tasks;
#endif

namespace socket4net
{
    public class RpcClient<TSession, TLogicService, TNetService> : Client<TSession, TLogicService, TNetService>
        where TSession : class, IRpcSession, new()
        where TLogicService : class, ILogicService, new()
        where TNetService : class, INetService, new()
    {
#if NET45
        public async Task<RpcResult> RequestAsync<T>(byte targetServer, long playerId, short ops, T proto, long objId,
            short componentId) where T : IProtobufInstance
        {
            var session = Session;
            if (session == null) return false;
            return await session.RequestAsync(targetServer, playerId, ops, proto, objId, componentId);
        }

        public async Task<RpcResult> RequestAsync(byte targetServer, long playerId, short ops, byte[] data, long objId,
            short componentId)
        {
            var session = Session;
            if (session == null) return false;
            return await session.RequestAsync(targetServer, playerId, ops, data, objId, componentId);
        }

        public async Task<RpcResult> RequestAsync(byte targetServer, long playerId, short ops, long objId, short componentId)
        {
            var session = Session;
            if (session == null) return false;
            return await session.RequestAsync(targetServer, playerId, ops, null, objId, componentId);
        }
#endif

        public void RequestAsync<T>(byte targetServer, long playerId, short ops, T proto, long objId, short componentId,
            Action<bool, byte[]> cb)
            where T : IProtobufInstance
        {
            var session = Session;
            if (session == null)
            {
                if (cb != null)
                    cb(false, null);

                return;
            }

            session.RequestAsync(targetServer, playerId, ops, proto, objId, componentId, cb);
        }

        public void RequestAsync(byte targetServer, long playerId, short ops, byte[] data, long objId, short componentId,
            Action<bool, byte[]> cb)
        {
            var session = Session;
            if (session == null)
            {
                if (cb != null)
                    cb(false, null);

                return;
            }

            session.RequestAsync(targetServer, playerId, ops, data, objId, componentId, cb);
        }

        public void RequestAsync(byte targetServer, long playerId, short ops, long objId, short componentId,
            Action<bool, byte[]> cb)
        {
            RequestAsync(targetServer, playerId, ops, null, objId, componentId, cb);
        }

        public void Push<T>(byte targetServer, long playerId, short ops, T proto, long objId, short componentId)
            where T : IProtobufInstance
        {
            var session = Session;
            if (session == null)
                return;

            session.Push(targetServer, playerId, ops, proto, objId, componentId);
        }

        public void Push(byte targetServer, long playerId, short ops, byte[] data, long objId, short componentId)
        {
            var session = Session;
            if (session == null)
                return;

            session.Push(targetServer, playerId, ops, data, objId, componentId);
        }

        public void Push<T>(long playerId, short ops, T proto, long objId, short componentId) where T : IProtobufInstance
        {
            Push(0, playerId, ops, proto, objId, componentId);
        }

        public void Push(long playerId, short ops, byte[] data, long objId, short componentId)
        {
            Push(0, playerId, ops, data, objId, componentId);
        }
    }

    public class Client<TSession> : RpcClient<TSession, AutoLogicService, NetService> where TSession : class, IRpcSession, new()
    {
    }

    public class UnityClient<TSession> : RpcClient<TSession, PassiveLogicService, NetService> where TSession : class, IRpcSession, new()
    {
    }
}