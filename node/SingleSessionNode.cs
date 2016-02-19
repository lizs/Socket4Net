using System;
using System.Linq;
using System.Threading.Tasks;
using socket4net;

namespace node
{
    /// <summary>
    ///     单会话节点
    ///     即：始终仅维护一条会话的节点，一般以客户端居多
    ///     如：Connector2GateClient
    /// </summary>
    /// <typeparam name="TSession"></typeparam>
    public abstract class SingleSessionNode<TCategory, TSession> : Node<TCategory, TSession> where TSession : class, IRpcSession, new()
    {
        private TSession _session;

        public TSession Session
        {
            get { return _session ?? (_session = Peer.SessionMgr.FirstOrDefault() as TSession); }
        }

        protected override void OnConnected(ISession session)
        {
            base.OnConnected(session);

            if (Peer.SessionMgr.Count > 1)
                Logger.Ins.Warn("单会话节点 : {0}，拥有多个会话", Name);
        }

        protected override void OnDisconnected(ISession session, SessionCloseReason reason)
        {
            base.OnDisconnected(session, reason);
            _session = null;
        }

        #region 对端

        //public async Task<RpcResult> RequestOppositeAsync(long playerId, short ops, byte[] data,
        //    long objId, short componentId)
        //{
        //    return await RequestAsync(TCategory.Opposite, playerId, ops, data, objId, componentId);
        //}

        //public void RequestOppositeAsync(long playerId, short ops, byte[] data, long objId,
        //    short componentId, Action<bool, byte[]> cb)
        //{
        //    RequestAsync(TCategory.Opposite, playerId, ops, data, objId, componentId, cb);
        //}

        //public bool PushOpposite(long playerId, short ops, byte[] data, long objId, short componentId)
        //{
        //    return Push(TCategory.Opposite, playerId, ops, data, objId, componentId);
        //}

        //public async Task<RpcResult> RequestOppositeAsync<T>(long playerId, short ops, T proto,
        //    long objId, short componentId)
        //{
        //    return await RequestAsync(TCategory.Opposite, playerId, ops, proto, objId, componentId);
        //}

        //public void RequestOppositeAsync<T>(long playerId, short ops, T proto, long objId,
        //    short componentId, Action<bool, byte[]> cb)
        //{
        //    RequestAsync(TCategory.Opposite, playerId, ops, proto, objId, componentId, cb);
        //}

        //public bool PushOpposite<T>(long playerId, short ops, T proto, long objId, short componentId)
        //   
        //{
        //    return Push(TCategory.Opposite, playerId, ops, proto, objId, componentId);
        //}

        #endregion

        #region 远端

        public async Task<RpcResult> RequestAsync(TCategory node, long playerId, short ops, byte[] data,
            long objId, short componentId)
        {
            if (Session == null) return RpcResult.Failure;
            return await Session.RequestAsync(CategoryConvertor(node), playerId, ops, data, objId, componentId);
        }

        public void RequestAsync(TCategory node, long playerId, short ops, byte[] data, long objId,
            short componentId, Action<bool, byte[]> cb)
        {
            if (Session == null)
            {
                cb(false, null);
                return;
            }

            Session.RequestAsync(CategoryConvertor(node), playerId, ops, data, objId, componentId, cb);
        }

        public bool Push(TCategory node, long playerId, short ops, byte[] data, long objId, short componentId)
        {
            if (Session == null) return false;
            Session.Push(CategoryConvertor(node), playerId, ops, data, objId, componentId);
            return true;
        }

        public async Task<RpcResult> RequestAsync<T>(TCategory node, long playerId, short ops, T proto,
            long objId, short componentId)
        {
            if (Session == null) return RpcResult.Failure;
            return
                await
                    Session.RequestAsync(CategoryConvertor(node), playerId, ops, PiSerializer.Serialize(proto), objId,
                        componentId);
        }

        public void RequestAsync<T>(TCategory node, long playerId, short ops, T proto, long objId,
            short componentId, Action<bool, byte[]> cb)
        {
            if (Session == null)
            {
                cb(false, null);
                return;
            }

            Session.RequestAsync(CategoryConvertor(node), playerId, ops, PiSerializer.Serialize(proto), objId, componentId, cb);
        }

        public bool Push<T>(TCategory node, long playerId, short ops, T proto, long objId, short componentId)
           
        {
            if (Session == null) return false;
            Session.Push(CategoryConvertor(node), playerId, ops, PiSerializer.Serialize(proto), objId, componentId);
            return true;
        }

        #endregion
    }
}