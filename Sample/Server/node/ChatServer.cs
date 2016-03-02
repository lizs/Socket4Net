using System;
using ecs;
using node;
using Shared;
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

            // 用会话Id作为玩家的唯一id（测试）
            // 正常情况是从身份验证服务器获取
            PlayerMgr.Ins.Create<Player>(
                new FlushablePlayerArg(PlayerMgr.Ins, session.Id, session, true,
                    () => RedisMgr<AsyncRedisClient>.Instance.GetFirst(x => x.Config.Type == "Chat")),
                true);

            // 通知客户端创建玩家
            session.Push(session.Id, (short)ENonPlayerOps.CreatePlayer, null, 0, 0);
        }

        protected override void OnDisconnected(ChatSession session, SessionCloseReason reason)
        {
            base.OnDisconnected(session, reason);
            PlayerMgr.Ins.Destroy(session.Id);
        }
    }
}
