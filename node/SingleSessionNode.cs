using System;
using System.Threading.Tasks;
using socket4net;

namespace node
{
    public interface ISingleSessionNode : INode
    {
        IRpcSession Session { get; }

        Task<RpcResult> RequestAsync(byte targetNode, long playerId, short ops, byte[] data,
            long objId, short componentId);

        void RequestAsync(byte targetNode, long playerId, short ops, byte[] data, long objId,
            short componentId, Action<bool, byte[]> cb);

        bool Push(byte targetNode, long playerId, short ops, byte[] data, long objId, short componentId);

        Task<RpcResult> RequestAsync<T>(byte targetNode, long playerId, short ops, T proto,
            long objId, short componentId);

        void RequestAsync<T>(byte targetNode, long playerId, short ops, T proto, long objId,
            short componentId, Action<bool, byte[]> cb);

        bool Push<T>(byte targetNode, long playerId, short ops, T proto, long objId, short componentId);
    }

    /// <summary>
    ///     单会话节点
    ///     即：始终仅维护一条会话的节点，一般以客户端居多
    ///     如：Connector2GateClient
    /// </summary>
    public abstract class SingleSessionNode<TSession> : Node<TSession>, ISingleSessionNode where TSession : class, IRpcSession, new()
    {
        private IRpcSession _session;
        public IRpcSession Session
        {
            get { return _session ?? (_session = Peer.SessionMgr.GetFirst<TSession>()); }
        }

        #region 远端

        public async Task<RpcResult> RequestAsync(byte targetNode, long playerId, short ops, byte[] data,
            long objId, short componentId)
        {
            if (Session == null) return RpcResult.Failure;
            return await Session.RequestAsync(targetNode, playerId, ops, data, objId, componentId);
        }

        public void RequestAsync(byte targetNode, long playerId, short ops, byte[] data, long objId,
            short componentId, Action<bool, byte[]> cb)
        {
            if (Session == null)
            {
                cb(false, null);
                return;
            }

            Session.RequestAsync(targetNode, playerId, ops, data, objId, componentId, cb);
        }

        public bool Push(byte targetNode, long playerId, short ops, byte[] data, long objId, short componentId)
        {
            if (Session == null) return false;
            Session.Push(targetNode, playerId, ops, data, objId, componentId);
            return true;
        }

        public async Task<RpcResult> RequestAsync<T>(byte targetNode, long playerId, short ops, T proto,
            long objId, short componentId)
        {
            if (Session == null) return RpcResult.Failure;
            return
                await
                    Session.RequestAsync(targetNode, playerId, ops, PiSerializer.Serialize(proto), objId,
                        componentId);
        }

        public void RequestAsync<T>(byte targetNode, long playerId, short ops, T proto, long objId,
            short componentId, Action<bool, byte[]> cb)
        {
            if (Session == null)
            {
                cb(false, null);
                return;
            }

            Session.RequestAsync(targetNode, playerId, ops, PiSerializer.Serialize(proto), objId, componentId, cb);
        }

        public bool Push<T>(byte targetNode, long playerId, short ops, T proto, long objId, short componentId)
        {
            if (Session == null) return false;
            Session.Push(targetNode, playerId, ops, PiSerializer.Serialize(proto), objId, componentId);
            return true;
        }

        #endregion
    }
}