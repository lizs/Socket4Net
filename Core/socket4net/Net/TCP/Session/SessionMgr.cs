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
        public IPeer Peer => Owner as IPeer;

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
                Peer.PerformInLogic(() =>
                {
                    if (_openCb == null) return;
                    _openCb(session);
                });
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
                    Peer.PerformInLogic(() =>
                    {
                        if(_closeCb == null) return;
                        _closeCb(session, reason);
                    });
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
