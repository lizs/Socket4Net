using System;
using ecs;
using socket4net;

namespace Sample
{
    public class Player : Player<AsyncRedisClient>
    {
        protected override EntitySys CreateEntitySys()
        {
            return Create<EntitySys>(new EntitySysArg(this, null, (l, s) => string.Format("{0}:{1}", l, s), s => 1));
        }

        protected override AsyncRedisClient RedisClient
        {
            get { return null; }
        }
    }

    public class PlayerMgr : UniqueMgr<long, Player>
    {
    }

    public class Server : Server<ChatSession>
    {
        public static Server Ins { get; private set; }

        public Server()
        {
            if(Ins != null)
                throw new Exception("Server already instantiated!");

            Ins = this;
        }
    }
}
