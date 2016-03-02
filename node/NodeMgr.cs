using System;
using System.Collections.Generic;
using socket4net;

namespace node
{
    public class NodesMgrArg : UniqueMgrArg
    {
        public LauncherConfig Config { get; private set; }
        public NodesMgrArg(IObj parent, LauncherConfig config)
            : base(parent)
        {
            Config = config;
        }
    }

    /// <summary>
    ///     节点管理器
    /// </summary>
    public abstract class NodesMgr : UniqueMgr<Guid, Node>
    {
        public LauncherConfig Config { get; private set; }
        private RedisMgr<AsyncRedisClient> _redisMgr;

        protected override void OnInit(ObjArg arg)
        {
            base.OnInit(arg);

            var more = arg.As<NodesMgrArg>();
            Config = more.Config;
            if (Config == null)
                throw new ArgumentNullException("arg");

            Logger.Ins.Info(Config.ToString());

            // 创建服务器
            if (!CreateNodes())
                throw new Exception("创建服务器失败");
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _redisMgr.Destroy();
        }

        public ServerNode<TSession> GetServer<TSession>() where TSession : class, IRpcSession, new()
        {
            return GetFirst<ServerNode<TSession>>();
        }

        public ClientNode<TSession> GetClient<TSession>() where TSession : class, IRpcSession, new()
        {
            return GetFirst<ClientNode<TSession>>();
        }

        public Node<TSession> GetNode<TSession>() where TSession : class, IRpcSession, new()
        {
            return GetFirst<Node<TSession>>();
        }

        public IEnumerable<T> GetSession<T>() where T : class, IRpcSession, new()
        {
            return GetFirst<Node<T>>().GetSession<T>();
        }

        public T GetSession<T>(long sid) where T : class, IRpcSession, new()
        {
            return GetFirst<Node<T>>().GetSession<T>(sid);
        }

        public IEnumerable<T> GetSession<T>(Predicate<T> condition) where T : class, IRpcSession, new()
        {
            return GetFirst<Node<T>>().GetSession<T>(condition);
        }

        public T GetFirstSession<T>(Predicate<T> condition) where T : class, IRpcSession, new()
        {
            return GetFirst<Node<T>>().GetFirstSession<T>(condition);
        }

        public T GetFirstSession<T>() where T : class, IRpcSession, new()
        {
            return GetFirst<Node<T>>().GetFirstSession<T>();
        }

        private bool CreateNodes()
        {
            if (Config == null) return false;

            _redisMgr = New<RedisMgr<AsyncRedisClient>>(new RedisMgrArg(this,
                Config.RedisNodes), true);

            foreach (var cfg in Config.Servers)
            {
                Create(cfg.NodeClass ?? typeof(ServerNode<>).MakeGenericType(new[] { MapSession(cfg.Type) }),
                    new NodeArg(this, cfg.Guid, cfg), false);
            }

            foreach (var cfg in Config.Clients)
            {
                Create(cfg.NodeClass ?? typeof(ClientNode<>).MakeGenericType(new[] { MapSession(cfg.Type) }),
                    new NodeArg(this, cfg.Guid, cfg), false);
            }

            return true;
        }

        protected abstract Type MapSession(string type);
    }
}
