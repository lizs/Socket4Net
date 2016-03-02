using System.Threading.Tasks;
using Shared;
using socket4net;

namespace Sample
{
    public class ChatComponent : ChatComponentBase
    {
        public async override Task<RpcResult> OnRequest(short ops, byte[] data)
        {
            switch ((EChatOps)ops)
            {
                case EChatOps.Echo:
                    {
                        var proto = PiSerializer.Deserialize<EchoProto>(data);
                        return
                            RpcResult.MakeSuccess(new EchoProto
                            {
                                Message = string.Format("Response from server : {0}", proto.Message)
                            });
                    }

                case EChatOps.Create:
                case EChatOps.Destroy:
                    return RpcResult.Failure;

                default:
                    return RpcResult.Failure;
            }
        }

        public async override Task<bool> OnPush(short ops, byte[] data)
        {
            switch ((EChatOps)ops)
            {
                case EChatOps.Broadcst:
                    {
                        var proto = PiSerializer.Deserialize<BroadcastProto>(data);

                        // 广播该消息至客户端ChatComponent
                        ChatNodesMgr.Ins.GetServer<ChatSession>()
                            .Broadcast(0, (short) EChatOps.Broadcst, new BroadcastProto
                            {
                                Message = Host.Name + " : " + proto.Message
                            }, Id);
                        return true;
                    }

                default:
                    return false;
            }
        }
    }
}
