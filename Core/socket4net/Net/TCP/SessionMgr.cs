using System;
using System.Linq;
#if NET45
using System.Collections.Concurrent;
#endif

namespace socket4net
{
    public class SessionMgrArg : UniqueMgrArg
    {
        public SessionMgrArg(IPeer peer, Action<ISession> openCb, Action<ISession, SessionCloseReason> closeCb)
            : base(peer)
        {
            OpenCallback = openCb;
            CloseCallback = closeCb;
        }

        public Action<ISession> OpenCallback { get; private set; }
        public Action<ISession, SessionCloseReason> CloseCallback { get; private set; }
    }

    public class SessionMgr : UniqueMgr<ConcurrentDictionary<long, ISession>, long, ISession> 
    {
        public IPeer Peer
        {
            get { return Owner as IPeer; }
        }

        private Action<ISession> _openCb;
        private Action<ISession, SessionCloseReason> _closeCb;
        
        protected override void OnInit(ObjArg arg)
        {
            base.OnInit(arg);
            var more = arg.As<SessionMgrArg>();

            _openCb = more.OpenCallback;
            _closeCb = more.CloseCallback;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Clear();

            _openCb = null;
            _closeCb = null;
        }

        public void AddSession(ISession session)
        {
            if (Items.TryAdd(session.Id, session))
            {
                Peer.PerformInLogic(() => _openCb(session));
            }
            else
                Logger.Ins.Warn("Insert session failed for id : " + session.Id);
        }
        
        public void RemoveSession(long id, SessionCloseReason reason)
        {
            ISession session;
            if (Items.TryRemove(id, out session))
            {
                if (Peer != null && _closeCb != null)
                    Peer.PerformInLogic(() => _closeCb(session, reason));
            }
            else if (Items.ContainsKey(id))
                Logger.Ins.Warn("Remove session failed for id : " + id);
            else
                Logger.Ins.Warn("Remove session failed for id :  cause of it doesn't exist" + id);
        }

        public void Clear()
        {
            foreach (var session in this.ToArray())
            {
                session.Close(SessionCloseReason.ClosedByMyself);
            }
        }
    }
}
