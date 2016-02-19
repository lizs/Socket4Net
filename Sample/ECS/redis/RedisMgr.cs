using System;
using System.Collections.Generic;
using node;
using socket4net;

namespace Sample
{
    public enum ERedisCategory
    {
        Account,
        Player,
        Fight,
        Rank,
        Mail,
        Activity,
        Ladder,
    }

    public class RedisMgrArg : UniqueMgrArg
    {
        public IEnumerable<RedisElement> Config { get; private set; }
        public RedisMgrArg(IObj parent, IEnumerable<RedisElement> cfg)
            : base(parent)
        {
            Config = cfg;
        }
    }

    public class RedisMgr<T> : UniqueMgr<short, T> where T : RedisClient, new()
    {
        public static RedisMgr<T> Instance { get; private set; }
        public RedisMgr()
        {
            if(Instance != null) throw new Exception("RedisMgr already instantiated!");
            Instance = this;
        }

        public IEnumerable<RedisElement> Config { get; private set; }

        protected override void OnInit(ObjArg arg)
        {
            base.OnInit(arg);

            var more = arg.As<RedisMgrArg>();
            Config = more.Config;

            foreach (var redisElement in Config)
            {
                Create<T>(new RedisClientArg(this, (short) redisElement.Category.To<ERedisCategory>(), redisElement.Ip,
                    redisElement.Port.ToString(),
                    redisElement.Pwd));
            }
        }
    }
}
