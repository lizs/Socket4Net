using System;
using System.Threading.Tasks;
using Proto;
using socket4net;

namespace WServer
{
    public class ChatSession : WebsocketSession
    {
        public override async Task<RpcResult> OnRequest(IDataProtocol rq)
        {
            var more = rq as DefaultDataProtocol;

            switch ((EOps)more.Ops)
            {
                case EOps.Reqeust:
                {
                    var proto = PiSerializer.Deserialize<RequestProto>(more.Data);
                    return RpcResult.MakeSuccess(new ResponseProto
                    {
                        Message = $"Response from server : {proto.Message}"
                    });
                }

                default:
                    return RpcResult.Failure;
            }
        }

        public override async Task<bool> OnPush(IDataProtocol ps)
        {
            var more = ps as DefaultDataProtocol;
            switch ((EOps)more.Ops)
            {
                case EOps.Push:
                {
                    var proto = PiSerializer.Deserialize<PushProto>(more.Data);

                    // ¹ã²¥Àý×Ó
                    return await BroadcastAsync(new DefaultDataProtocol
                    {
                        Ops = (short) EOps.Push,
                        Data = PiSerializer.Serialize(new PushProto
                        {
                            Message = ID + " : " + proto.Message
                        })
                    });
                }

                default:
                    return RpcResult.Failure;
            }
        }
    }
}