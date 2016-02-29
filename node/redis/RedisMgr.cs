using System;
using System.Collections.Generic;
using socket4net;

namespace node
{
    public class RedisMgrArg : UniqueMgrArg
    {
        public IEnumerable<NodeElement> Config { get; private set; }
        public RedisMgrArg(IObj parent, IEnumerable<NodeElement> cfg)
            : base(parent)
        {
            Config = cfg;
        }
    }

    public class RedisMgr<T> : UniqueMgr<string, T> where T : RedisClient, new()
    {
        public static RedisMgr<T> Instance { get; private set; }
        public RedisMgr()
        {
            if(Instance != null) throw new Exception("RedisMgr already instantiated!");
            Instance = this;
        }

        public IEnumerable<NodeElement> Config { get; private set; }

        protected override void OnInit(ObjArg arg)
        {
            base.OnInit(arg);

            var more = arg.As<RedisMgrArg>();
            Config = more.Config;

            foreach (var redisElement in Config)
            {
                Create<T>(new RedisClientArg(this, redisElement.Type, redisElement), false);
            }
        }
    }
}
