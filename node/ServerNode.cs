using System.Linq;
using socket4net;

namespace node
{
    /// <summary>
    ///     服务器节点
    /// </summary>
    /// <typeparam name="TSession"></typeparam>
    /// <typeparam name="TCategory"></typeparam>
    public abstract class ServerNode<TCategory, TSession> : Node<TCategory, TSession> where TSession : class, IRpcSession, new()
    {
        protected override void OnInit(ObjArg arg)
        {
            base.OnInit(arg);

            var more = arg.As<NodeArg<TCategory>>();
            Peer = Create<Server<TSession>>(new PeerArg(null, Config.Ip, Config.Port), false);
        }

        protected override void OnStart()
        {
            base.OnStart();
            Peer.Start();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (Peer == null) return;
            Peer.Destroy();
        }

        public void Broadcast(TCategory node, short ops, byte[] data, short componentId)
        {
            foreach (var session in Peer.SessionMgr.Cast<IRpcSession>())
            {
                session.Push(CategoryConvertor(node), 0, ops, data, 0, componentId);
            }
        }

        public void Broadcast<T>(TCategory node, short ops, T proto, short componentId)
        {
            Broadcast(node, ops, PiSerializer.Serialize(proto), componentId);
        }
    }
}