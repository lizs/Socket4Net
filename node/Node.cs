using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using socket4net;

namespace node
{
    public class NodeArg<TCategory> : UniqueObjArg<Guid>
    {
        public NodeArg(IObj parent, Guid key, NodeElement cfg, Func<TCategory, byte> convertor)
            : base(parent, key)
        {
            Config = cfg;
            CategoryConvertor = convertor;
        }

        public NodeElement Config { get; private set; }
        public Func<TCategory, byte> CategoryConvertor { get; private set; } 
    }

    /// <summary>
    ///     节点
    /// </summary>
    public abstract class Node<TCategory> : UniqueObj<Guid>
    {
        /// <summary>
        ///     名
        /// </summary>
        public override string Name
        {
            get
            {
                return string.Format("{0}:{1}:{2}", GetType().Name, Config.Name,
                    Peer != null ? string.Format("{0}:{1}", Peer.Ip, Peer.Port) : "空");
            }
        }

        /// <summary>
        ///     节点类型
        /// </summary>
        public string Category
        {
            get { return Config.Category; }
        }

        /// <summary>
        ///     Peer
        /// </summary>
        public IPeer Peer { get; protected set; }

        /// <summary>
        ///     配置
        /// </summary>
        public NodeElement Config { get; private set; }

        /// <summary>
        ///     类型转化
        /// </summary>
        protected Func<TCategory, byte> CategoryConvertor;

        /// <summary>
        ///     获取配置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetConfig<T>() where T : NodeElement
        {
            return Config as T;
        }

        public IEnumerable<T> GetSession<T>() where T : ISession
        {
            return Peer == null ? null : Peer.SessionMgr.Get<T>();
        }

        public T GetSession<T>(long sid) where T : class, ISession
        {
            return Peer == null ? null : Peer.SessionMgr.Get(sid) as T;
        }

        public IEnumerable<T> GetSession<T>(Predicate<T> condition) where T : ISession
        {
            return Peer == null ? null : Peer.SessionMgr.Get<T>().Where(x => condition(x));
        }

        public T GetFirstSession<T>(Predicate<T> condition) where T : ISession
        {
            var sessions = GetSession(condition);
            return sessions == null ? default(T) : sessions.FirstOrDefault();
        }

        public T GetFirstSession<T>() where T : ISession
        {
            var sessions = GetSession<T>();
            return sessions == null ? default(T) : sessions.FirstOrDefault();
        }

        protected override void OnInit(ObjArg arg)
        {
            base.OnInit(arg);

            Config = arg.As<NodeArg<TCategory>>().Config;
        }
    }

    public abstract class Node<TCategory, TSession> : Node<TCategory> where TSession : class, IRpcSession, new()
    {
        protected override void OnStart()
        {
            base.OnStart();

            Peer.EventSessionClosed += OnDisconnected;
            Peer.EventSessionEstablished += OnConnected;
            Peer.EventErrorCatched += OnError;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            Peer.EventSessionClosed -= OnDisconnected;
            Peer.EventSessionEstablished -= OnConnected;
            Peer.EventErrorCatched -= OnError;
        }

        protected virtual void OnConnected(ISession session)
        {
        }

        protected virtual void OnDisconnected(ISession session, SessionCloseReason reason)
        {
        }

        protected virtual void OnError(string msg)
        {
        }

        #region

        public async Task<RpcResult> RequestAsync<T>(long sessionId, TCategory targetServer, long playerId, short ops,
            T proto, long objId, short componentId)
        {
            return
                await
                    RequestAsync(sessionId, targetServer, playerId, ops, PiSerializer.Serialize(proto), objId,
                        componentId);
        }

        public async Task<RpcResult> RequestAsync(long sessionId, TCategory targetServer, long playerId, short ops,
            byte[] data, long objId, short componentId)
        {
            var session = Peer.SessionMgr.Get(sessionId) as TSession;
            if (session == null) return RpcResult.Failure;
            return await session.RequestAsync(CategoryConvertor(targetServer), playerId, ops, data, objId, componentId);
        }

        public void RequestAsync<T>(long sessionId, TCategory targetServer, long playerId, short ops,
            T proto, long objId, short componentId, Action<bool, byte[]> cb)
        {
            RequestAsync(sessionId, targetServer, playerId, ops, PiSerializer.Serialize(proto), objId, componentId, cb);
        }

        public void RequestAsync(long sessionId, TCategory targetServer, long playerId, short ops,
            byte[] data, long objId, short componentId, Action<bool, byte[]> cb)
        {
            var session = Peer.SessionMgr.Get(sessionId) as TSession;
            if (session == null)
            {
                cb(false, null);
                return;
            }

            session.RequestAsync(CategoryConvertor(targetServer), playerId, ops, data, objId, componentId, cb);
        }

        public bool Push<T>(long sessionId, TCategory targetServer, long playerId, short ops, T proto, long objId,
            short componentId)
        {
            return Push(sessionId, targetServer, playerId, ops, PiSerializer.Serialize(proto), objId, componentId);
        }

        public bool Push(long sessionId, TCategory targetServer, long playerId, short ops, byte[] data, long objId,
            short componentId)
        {
            var session = Peer.SessionMgr.Get(sessionId) as TSession;
            if (session == null) return false;
            session.Push(CategoryConvertor(targetServer), playerId, ops, data, objId, componentId);
            return true;
        }

        #endregion
    }
}