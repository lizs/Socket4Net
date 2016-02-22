using System;
using ecs;
using node;
using socket4net;

namespace Sample
{
    public enum ENode
    {
        Chat,
    }

    public enum ECategory
    {
        Chat,
    }

    public class MyServer : ServerNode<ENode, ChatSession>
    {
        public static MyServer Ins { get; private set; }

        public MyServer()
        {
            if(Ins != null)
                throw new Exception("MyServer already instantiated!");

            Ins = this;
        }

        protected override void OnConnected(ISession session)
        {
            base.OnConnected(session);

            var cs = (ChatSession)session;
            cs.Player = PlayerMgr.Ins.Create<Player>(
                new FlushablePlayerArg(PlayerMgr.Ins, session.Id, cs, true,
                    () => RedisMgr<AsyncRedisClient>.Instance.Get((short)ECategory.Chat)),
                true);
        }

        protected override void OnDisconnected(ISession session, SessionCloseReason reason)
        {
            base.OnDisconnected(session, reason);
            PlayerMgr.Ins.Destroy(session.Id);
        }
    }
}
