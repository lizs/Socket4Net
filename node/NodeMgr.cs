using System;
using socket4net;

namespace node
{
    public interface IServerConfig
    {
        //Guid Id { get; }
    }

    public class NodesMgrArg : UniqueMgrArg
    {
        public IServerConfig Config { get; private set; }
        public NodesMgrArg(IObj parent, IServerConfig config)
            : base(parent)
        {
            Config = config;
        }
    }

    /// <summary>
    ///     节点管理器
    /// </summary>
    public abstract class NodesMgr<TCategory> : UniqueMgr<Guid, Node<TCategory>>
    {
        public IServerConfig Config { get; private set; }
        
        protected override void OnInit(ObjArg arg)
        {
            base.OnInit(arg);

            var more = arg.As<NodesMgrArg>();
            Config = more.Config;
            if(Config == null)
                throw new Exception("NodeMgr's config is null!");
            Logger.Ins.Info(Config.ToString());

            if (!CreateInternal())
                throw new Exception();
        }

        protected abstract bool CreateInternal();
    }
}
