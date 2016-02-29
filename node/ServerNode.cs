using System.Linq;
using socket4net;

namespace node
{
    /// <summary>
    ///     服务器节点
    /// </summary>
    /// <typeparam name="TSession"></typeparam>
    public class ServerNode<TSession> : Node<TSession> where TSession : class, IRpcSession, new()
    {
        protected override void OnInit(ObjArg arg)
        {
            base.OnInit(arg);
            Peer = Create<Server<TSession>>(new PeerArg(null, Config.Ip, Config.Port));
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

        public void Broadcast(byte node, short ops, byte[] data, short componentId)
        {
            foreach (var session in Peer.SessionMgr.Cast<IRpcSession>())
            {
                session.Push(node, 0, ops, data, 0, componentId);
            }
        }

        public void Broadcast<T>(byte node, short ops, T proto, short componentId)
        {
            Broadcast(node, ops, PiSerializer.Serialize(proto), componentId);
        }
    }
}