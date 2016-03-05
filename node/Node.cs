#region MIT
//  /*The MIT License (MIT)
// 
//  Copyright 2016 lizs lizs4ever@163.com
//  
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//  
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
//  
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
//   * */
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using socket4net;

namespace node
{
    public class NodeArg : UniqueObjArg<Guid>
    {
        public NodeArg(IObj parent, Guid key, NodeElement cfg)
            : base(parent, key)
        {
            Config = cfg;
        }

        public NodeElement Config { get; private set; }
    }

    public interface INode : IObj
    {
        IEnumerable<T> GetSession<T>() where T : ISession;
        T GetSession<T>(long sid) where T : class, ISession;
        IEnumerable<T> GetSession<T>(Predicate<T> condition) where T : ISession;
        T GetFirstSession<T>(Predicate<T> condition) where T : ISession;
        T GetFirstSession<T>() where T : ISession;

        Task<RpcResult> RequestAsync<T>(long sessionId, byte targetServer, long playerId, short ops,
            T proto, long objId, short componentId);

        Task<RpcResult> RequestAsync(long sessionId, byte targetServer, long playerId, short ops,
            byte[] data, long objId, short componentId);

        void RequestAsync<T>(long sessionId, byte targetServer, long playerId, short ops,
            T proto, long objId, short componentId, Action<bool, byte[]> cb);

        void RequestAsync(long sessionId, byte targetServer, long playerId, short ops,
            byte[] data, long objId, short componentId, Action<bool, byte[]> cb);

        bool Push<T>(long sessionId, byte targetServer, long playerId, short ops, T proto, long objId,
            short componentId);

        bool Push(long sessionId, byte targetServer, long playerId, short ops, byte[] data, long objId,
            short componentId);
    }

    /// <summary>
    ///     节点
    /// </summary>
    public abstract class Node : UniqueObj<Guid>, INode
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
        public string Type
        {
            get { return Config.Type; }
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
        ///     获取配置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetConfig<T>() where T : NodeElement
        {
            return Config as T;
        }

        /// <summary>
        ///     获取会话集
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="condition"></param>
        /// <returns></returns>
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

            Config = arg.As<NodeArg>().Config;
        }

        #region

        public async Task<RpcResult> RequestAsync<T>(long sessionId, byte targetServer, long playerId, short ops,
            T proto, long objId, short componentId)
        {
            return
                await
                    RequestAsync(sessionId, targetServer, playerId, ops, PiSerializer.Serialize(proto), objId,
                        componentId);
        }

        public async Task<RpcResult> RequestAsync(long sessionId, byte targetServer, long playerId, short ops,
            byte[] data, long objId, short componentId)
        {
            var session = Peer.SessionMgr.Get(sessionId) as IRpcSession;
            if (session == null) return RpcResult.Failure;
            return await session.RequestAsync(targetServer, playerId, ops, data, objId, componentId);
        }

        public void RequestAsync<T>(long sessionId, byte targetServer, long playerId, short ops,
            T proto, long objId, short componentId, Action<bool, byte[]> cb)
        {
            RequestAsync(sessionId, targetServer, playerId, ops, PiSerializer.Serialize(proto), objId, componentId, cb);
        }

        public void RequestAsync(long sessionId, byte targetServer, long playerId, short ops,
            byte[] data, long objId, short componentId, Action<bool, byte[]> cb)
        {
            var session = Peer.SessionMgr.Get(sessionId) as IRpcSession;
            if (session == null)
            {
                cb(false, null);
                return;
            }

            session.RequestAsync((byte)targetServer, playerId, ops, data, objId, componentId, cb);
        }

        public bool Push<T>(long sessionId, byte targetServer, long playerId, short ops, T proto, long objId,
            short componentId)
        {
            return Push(sessionId, targetServer, playerId, ops, PiSerializer.Serialize(proto), objId, componentId);
        }

        public bool Push(long sessionId, byte targetServer, long playerId, short ops, byte[] data, long objId,
            short componentId)
        {
            var session = Peer.SessionMgr.Get(sessionId) as IRpcSession;
            if (session == null) return false;
            session.Push((byte)targetServer, playerId, ops, data, objId, componentId);
            return true;
        }

        #endregion
    }

    public abstract class Node<TSession> : Node where TSession : class, IRpcSession, new()
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

        private void OnConnected(ISession session)
        {
            OnConnected((TSession)session);
        }

        private void OnDisconnected(ISession session, SessionCloseReason reason)
        {
            OnDisconnected((TSession)session, reason);
        }

        protected virtual void OnConnected(TSession session)
        {
        }

        protected virtual void OnDisconnected(TSession session, SessionCloseReason reason)
        {
        }

        protected virtual void OnError(string msg)
        {
        }
    }
}