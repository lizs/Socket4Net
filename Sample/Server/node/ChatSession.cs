using System.Threading.Tasks;
using socket4net;

namespace Sample
{
    public class ChatSession : RpcSession
    {
        public ChatSession()
        {
            ReceiveBufSize = 10 * 1024;
            PackageMaxSize = 40 * 1024;
        }

        public Player Player { get; set; }

        public override Task<RpcResult> HandleRequest(RpcRequest rq)
        {
            return Player == null ? Task.FromResult(RpcResult.Failure) : Player.OnRequest(rq);
        }

        //private void Broadcast(string msg)
        //{
        //    var proto = new PushMsgProto()
        //    {
        //        From = Id.ToString(),
        //        Message = msg,
        //    };

        //    var responseData = PiSerializer.Serialize(proto);

        //    foreach (var session in ChatServer.Ins.SessionMgr.OfType<IRpcSession>())
        //    {
        //        session.Push(0, 0, (short) EOps.Push, responseData, 0, 0);
        //    }
        //}

        public override Task<bool> HandlePush(RpcPush rp)
        {
            return Player == null ? Task.FromResult(false) : Player.OnPush(rp);
        }
    }
}