using System.Linq;
using System.Threading.Tasks;
using socket4net;

namespace ecs
{
    public abstract class ClientSession : DispatchableSession
    {
        protected override sealed Player GetPlayer(long playerId)
        {
            return playerId == 0 ? PlayerMgr.Ins.FirstOrDefault() : PlayerMgr.Ins.Get(playerId);
        }
    }

    public abstract class ServerSession : DispatchableSession
    {
        protected override sealed Player GetPlayer(long playerId)
        {
            return PlayerMgr.Ins.Get(playerId);
        }
    }

    public abstract class DispatchableSession : RpcSession
    {
        protected DispatchableSession()
        {
            ReceiveBufSize = 10 * 1024;
            PackageMaxSize = 40 * 1024;
        }

        protected abstract Player GetPlayer(long playerId);

        protected virtual Task<RpcResult> OnNonPlayerRequest(RpcRequest rq)
        {
            return Task.FromResult(RpcResult.Failure);
        }

        protected virtual Task<bool> OnNonPlayerPush(RpcPush rp)
        {
            return Task.FromResult(false);
        }
        
        public async override Task<RpcResult> HandleRequest(RpcRequest rq)
        {
            if (rq.Ops < 0)
            {
                return await OnNonPlayerRequest(rq);
            }

            var player = GetPlayer(rq.PlayerId);
            if (player == null)
                return false;

            using (new Flusher(player as IFlushable))
            {
                var entity = rq.ObjId != 0 ? player.Es.Get(rq.ObjId) : player;
                if (entity == null) return RpcResult.Failure;

                if (rq.ComponentId == 0)
                    return await entity.OnRequest(rq.Ops, rq.Data);

                var cp = entity.GetComponent(rq.ComponentId);
                return cp == null
                    ? RpcResult.Failure
                    : await cp.OnRequest(rq.Ops, rq.Data);
            }
        }

        public async override Task<bool> HandlePush(RpcPush rp)
        {
            if (rp.Ops < 0)
            {
                return await OnNonPlayerPush(rp);
            }

            var player = GetPlayer(rp.PlayerId);
            if (player == null)
                return false;

            using (new Flusher(player as IFlushable))
            {
                var entity = rp.ObjId != 0 ? player.Es.Get(rp.ObjId) : player;
                if (entity == null) return false;

                if (rp.ComponentId == 0)
                    return await entity.OnPush(rp.Ops, rp.Data);

                var cp = entity.GetComponent(rp.ComponentId);
                return cp != null && await cp.OnPush(rp.Ops, rp.Data);
            }
        }
    }
}