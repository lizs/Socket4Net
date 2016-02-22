using System;
using node;

namespace Sample
{
    public class MyNodesMgr : NodesMgr<ENode>
    {
        public MyNodesMgr()
        {
            if (_ins != null) throw new Exception("MyNodesMgr already instantiated!");
            _ins = this;
        }

        private static MyNodesMgr _ins;
        public static MyNodesMgr Ins { get { return _ins; } }
        private RedisMgr<AsyncRedisClient> _redisMgr;

        protected override bool CreateInternal()
        {
            var more = Config as ChatConfig;

            var client =
                Create<MyServer>(
                    new NodeArg<ENode>(this, more.Chat.Guid, more.Chat, node => (byte)node), false);

            return client != null;
        }

        protected override void OnStart()
        {
            base.OnStart();

            var more = Config as ChatConfig;
            _redisMgr = Create<RedisMgr<AsyncRedisClient>>(new RedisMgrArg(this,
                    more.Chat.RedisNodes), true);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _redisMgr.Destroy();
        }
    }
}
