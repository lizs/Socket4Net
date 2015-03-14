using System;
using System.Collections.Generic;
using System.Linq;
using Core.Log;
using Core.Service;
#if !NET35
using System.Collections.Concurrent;
#else
using Core.ConcurrentCollection;
#endif

namespace Core.Net.TCP
{
    public static class SessionMgr
    {
        private static readonly ConcurrentDictionary<long, ISession> _sessions = new ConcurrentDictionary<long, ISession>();
        public static Action<ISession, SessionCloseReason> EventSessionClosed;
        public static Action<ISession> EventSessionEstablished;
        public static int Count
        {
            get { return _sessions.Count; }
        }

        public static void Add(ISession session)
        {
            if (_sessions.TryAdd(session.Id, session))
            {
                Launcher.PerformInSta(() =>
                {
                    if (EventSessionEstablished != null)
                        EventSessionEstablished(session);
                });
            }
            else
                Log.Logger.Instance.Warn("Add session failed for id : " + session.Id);
        }

        public static void Remove(long id, SessionCloseReason reason)
        {
            ISession session;
            if (_sessions.TryRemove(id, out session))
            {
                Launcher.PerformInSta(() =>
                {
                    if (EventSessionClosed != null)
                        EventSessionClosed(session, reason);
                });
            }
            else if (_sessions.ContainsKey(id))
                Logger.Instance.Warn("Remove session failed for id : " + id);
            else
                Logger.Instance.Warn("Remove session failed for id :  cause of it doesn't exist" + id);
        }

        public static ISession Get(long id)
        {
            ISession session;
            return _sessions.TryGetValue(id, out session) ? session : null;
        }

        public static void Clear()
        {
            foreach (var session in Sessions)
            {
                Remove(session.Id, SessionCloseReason.ClosedByMyself);
            }
        }

        public static IEnumerable<ISession> Sessions
        {
            get
            {
                return _sessions.Select(x => x.Value);
            }
        }
    }
}
