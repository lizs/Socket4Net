using System.Threading.Tasks;
using ecs;
using Proto;
using socket4net;

namespace Sample
{
    public class Player : FlushablePlayer
    {
        protected override EntitySys CreateEntitySys()
        {
            return
                Create<EntitySys>(new EntitySysArg(this,
                    BlockMaker.Create,
                    (l, s) => string.Format("{0}:{1}", l, (EPid) s),
                    s =>
                    {
                        var items = s.Split(':');
                        return (short)items[2].To<EPid>();
                    }));
        }

        protected override void OnStart()
        {
            base.OnStart();
            Logger.Ins.Debug("{0} online!", Name);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Logger.Ins.Debug("{0} offline!", Name);
        }

        public async override Task<RpcResult> OnMessageAsync(AsyncMsg msg)
        {
            switch (msg.Type)
            {
                case EMsg.NetReq:
                {
                    var more = msg as NetReqMsg;
                    switch ((EOps) more.Ops)
                    {
                        case EOps.Request:
                        {
                            var proto = PiSerializer.Deserialize<RequestMsgProto>(more.Data);
                            return RpcResult.MakeSuccess(new ResponseMsgProto{Message = "Response from server!"});
                        }

                        default:
                            return RpcResult.Failure;
                    }
                }

                case EMsg.NetPush:
                {
                    var more = msg as NetPushMsg;
                    switch ((EOps)more.Ops)
                    {
                        case EOps.Push:
                        {
                            var proto = PiSerializer.Deserialize<PushMsgProto>(more.Data);
                            // 广播该消息
                            ChatServer.Ins.Broadcast(0, (short)EOps.Push, proto, 0);
                            return RpcResult.Success;
                        }

                        default:
                            return RpcResult.Failure;
                    }
                }
            }

            return RpcResult.Failure;
        }
    }
}