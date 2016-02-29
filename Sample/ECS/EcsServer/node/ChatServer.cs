using System;
using ecs;
using node;
using socket4net;

namespace Sample
{
    public class ChatServer : ServerNode<ChatSession>
    {
        public static ChatServer Ins { get; private set; }

        public ChatServer()
        {
            if(Ins != null)
                throw new Exception("ChatServer already instantiated!");

            Ins = this;
        }

        protected override void OnConnected(ChatSession session)
        {
            base.OnConnected(session);

            session.Player = PlayerMgr.Ins.Create<Player>(
                new FlushablePlayerArg(PlayerMgr.Ins, session.Id, session, true,
                    () => RedisMgr<AsyncRedisClient>.Instance.GetFirst(x=>x.Config.Type == "Chat")),
                true);
        }

        protected override void OnDisconnected(ChatSession session, SessionCloseReason reason)
        {
            base.OnDisconnected(session, reason);
            PlayerMgr.Ins.Destroy(session.Id);
        }
    }
}
