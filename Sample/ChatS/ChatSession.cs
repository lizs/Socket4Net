using System.Linq;
using System.Threading.Tasks;
using socket4net;
using Proto;

namespace ChatS
{
    public class ChatSession : RpcSession
    {
        public ChatSession()
        {
            ReceiveBufSize = 10 * 1024;
            PackageMaxSize = 40 * 1024;
        }

        public override Task<RpcResult> HandleRequest(RpcRequest rq)
        {
            switch ((ECommand)rq.Ops)
            {
                case ECommand.Request:
                    {
                        var request = PiSerializer.Deserialize<RequestMsgProto>(rq.Data);

                        return
                            Task.FromResult(
                                RpcResult.MakeSuccess(new ResponseMsgProto {Message = "Response : " + request.Message}));
                    }

                default:
                    return null;
            }
        }

        private void Broadcast(string msg)
        {
            var proto = new PushMsgProto()
            {
                From = Id.ToString(),
                Message = msg,
            };

            var responseData = PiSerializer.Serialize(proto);

            // ¹ã²¥
            foreach (var session in Server.Ins.SessionMgr.OfType<IRpcSession>())
            {
                session.Push(0, 0, (short) ECommand.Push, responseData, 0, 0);
            }
        }

        public override Task<bool> HandlePush(RpcPush rp)
        {
            switch ((ECommand)rp.Ops)
            {
                case ECommand.Push:
                    {
                        var msg = PiSerializer.Deserialize<PushMsgProto>(rp.Data);
                        Broadcast(msg.Message);
                        return Task.FromResult(true);
                    }

                default:
                    return null;
            }
        }
    }
}