using System.Threading.Tasks;
using ecs;
using Shared;
using socket4net;

namespace Sample
{
    public class SampleComponent : SampleComponentBase
    {
        public async override Task<RpcResult> OnRequest(short ops, byte[] data)
        {
            switch ((EOps)ops)
            {
                case EOps.Echo:
                {
                    var proto = PiSerializer.Deserialize<EchoProto>(data);
                    return
                        RpcResult.MakeSuccess(new EchoProto
                        {
                            Message = string.Format("Response from server : {0}", proto.Message)
                        });
                }

                case EOps.Create:
                {
                    var player = GetAncestor<Player>();
                    player.Es.CreateDefault<Ship>(new EntityArg(this, Uid.New()), true);
                    return true;
                }

                case EOps.Destroy:
                {
                    var player = GetAncestor<Player>();
                    player.Es.DestroyAll();
                    return true;
                }

                default:
                    return RpcResult.Failure;
            }
        }

        public async override Task<bool> OnPush(short ops, byte[] data)
        {
            switch ((EOps)ops)
            {
                case EOps.Broadcst:
                    {
                        var proto = PiSerializer.Deserialize<BroadcastProto>(data);

                        // 广播该消息至客户端ChatComponent
                        ServerNodesMgr.Ins.GetServer<SampleSession>()
                            .Broadcast(0, (short) EOps.Broadcst, new BroadcastProto
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
