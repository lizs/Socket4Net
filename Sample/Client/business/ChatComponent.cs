using System.Threading.Tasks;
using Shared;
using socket4net;

namespace Sample
{
    public class ChatComponent : ChatComponentBase
    {
        public async Task<bool> Echo(string msg)
        {
            var ret =
                await
                    RequestAsync(0, GetAncestor<Player>().Id, (short) EChatOps.Echo, new EchoProto {Message = msg},
                        0, Id);

            if (ret)
            {
                var proto = PiSerializer.Deserialize<EchoProto>(ret.Value);
                Logger.Ins.Info(proto.Message);
            }
            else
                Logger.Ins.Error("Echo failed!");

            return ret;
        }

        public void Broadcast(string msg)
        {
            Push(0, GetAncestor<Player>().Id, (short)EChatOps.Broadcst, new BroadcastProto { Message = msg },
                0, Id);
        }

        public async override Task<RpcResult> OnRequest(short ops, byte[] data)
        {
            switch ((EChatOps)ops)
            {
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
                        Logger.Ins.Info(proto.Message);
                        return true;
                    }

                default:
                    return false;
            }
        }
    }
}
