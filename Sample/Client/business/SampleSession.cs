using System.Threading.Tasks;
using ecs;
using Shared;
using socket4net;

namespace Sample
{
    public class SampleSession : ClientSession
    {
        protected override Task<bool> OnNonPlayerPush(RpcPush rp)
        {
            switch ((ENonPlayerOps)rp.Ops)
            {
                case ENonPlayerOps.CreatePlayer:
                {
                    // 创建该玩家
                    var player = PlayerMgr.Ins.CreateDefault<Player>(new PlayerArg(PlayerMgr.Ins, rp.PlayerId, this),
                        true);
                    return Task.FromResult(player != null);
                }

                default:
                    return Task.FromResult(false);
            }
        }
    }
}
