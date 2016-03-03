using System;
using ecs;
using node;
using Shared;
using socket4net;

namespace Sample
{
    public class Server : ServerNode<SampleSession>
    {
        public static Server Ins { get; private set; }

        public Server()
        {
            if(Ins != null)
                throw new Exception("Server already instantiated!");

            Ins = this;
        }

        protected override void OnConnected(SampleSession session)
        {
            base.OnConnected(session);

            // 服务器创建玩家
            var player = PlayerMgr.Ins.Create<Player>(
                new FlushablePlayerArg(PlayerMgr.Ins, Uid.New(), session, true,
                    () => RedisMgr<AsyncRedisClient>.Instance.GetFirst(x => x.Config.Type == "Sample")),
                true);

            // 1、通知客户端创建玩家
            session.Push(player.Id, (short)ENonPlayerOps.CreatePlayer, null, 0, 0);

            // 2、下发数据
            player.Flush();
        }

        protected override void OnDisconnected(SampleSession session, SessionCloseReason reason)
        {
            base.OnDisconnected(session, reason);
            PlayerMgr.Ins.Destroy(session.Id);
        }
    }
}
