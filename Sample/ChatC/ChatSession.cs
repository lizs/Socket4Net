using System.Threading.Tasks;
using socket4net;
using Proto;

namespace ChatC
{
    /// <summary>
    /// 聊天者客户端实现
    /// </summary>
    public class ChatSession : RpcSession
    {
        public ChatSession()
        {
            ReceiveBufSize = 10 * 1024;
            PackageMaxSize = 40 * 1024;
        }

        public override Task<RpcResult> HandleRequest(RpcRequest rq)
        {
            return Task.FromResult(RpcResult.Failure);
        }

        public override Task<bool> HandlePush(RpcPush rp)
        {
            switch ((EOps)rp.Ops)
            {
                case EOps.Push:
                    {
                        var msg = PiSerializer.Deserialize<PushMsgProto>(rp.Data);
                        Logger.Ins.Info(msg.From + " : " + msg.Message);
                        return Task.FromResult(true);
                    }

                default:
                    return Task.FromResult(false);
            }
        }
    }
}