using System;
#if NET45
using System.Threading.Tasks;
#endif
using socket4net;

namespace ecs
{
    public abstract class RpcComponent : Component, IRpc
    {
        #region IRpc Members
        public IRpcSession Session
        {
            get { return GetAncestor<Player>().Session; }
        }

#if NET35
        public virtual RpcResult HandleRequest(short ops, byte[] bytes)
        {
            Logger.Ins.Error("Rpc组件 {0} 收到未处理的Request : {1}, ", Name, ops);
            return RpcResult.Failure;
        }

        public virtual bool HandlePush(short ops, byte[] bytes)
        {
            Logger.Ins.Error("Rpc组件 {0} 收到未处理的Push : {1}, ", Name, ops);
            return false;
        }
#else
        public virtual Task<RpcResult> HandleRequest(short ops, byte[] bytes)
        {
            Logger.Ins.Error("Rpc组件 {0} 收到未处理的Request : {1}, ", Name, ops);
            return Task.FromResult(RpcResult.Failure);
        }

        public virtual Task<bool> HandlePush(short ops, byte[] data)
        {
            Logger.Ins.Error("Rpc组件 {0} 收到未处理的Push : {1}, ", Name, ops);
            return Task.FromResult(false);
        }
#endif

        #endregion

        #region 请求对端指定对象、指定组件
#if NET45
        public Task<RpcResult> RequestAsync<T>(byte targetServer, long playerId, short ops, T proto, long objId,
            short cpId)
        {
            return Session.RequestAsync(targetServer, playerId, ops, proto, objId, cpId);
        }

        public Task<RpcResult> RequestAsync(byte targetServer, long playerId, short ops, byte[] data, long objId,
            short cpId)
        {
            return Session.RequestAsync(targetServer, playerId, ops, data, objId, cpId);
        }
#endif

        public void RequestAsync<T>(byte targetServer, long playerId, short ops, T proto, long objId, short cpId,
            Action<bool, byte[]> cb)
        {
            Session.RequestAsync(targetServer, playerId, ops, proto, objId, cpId, cb);
        }

        public void RequestAsync(byte targetServer, long playerId, short ops, byte[] data, long objId, short cpId,
            Action<bool, byte[]> cb)
        {
            Session.RequestAsync(targetServer, playerId, ops, data, objId, cpId, cb);
        }

        #endregion

        #region 推送给对端指定对象的指定组件
        public void Push(byte targetServer, long playerId, short ops, byte[] data, long objId, short cpId)
        {
            Session.Push(targetServer, playerId, ops, data, objId, cpId);
        }

        public void Push<T>(byte targetServer, long playerId, short ops, T proto, long objId, short cpId)
        {
            Session.Push(targetServer, playerId, ops, proto, objId, cpId);
        }
        #endregion
    }
}