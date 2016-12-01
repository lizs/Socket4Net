using System;
using System.Threading.Tasks;
using Proto;
using socket4net;

namespace WServer
{
    public class ChatSession : WebsocketSession
    {
        /// <summary>
        ///     handle request
        /// </summary>
        /// <param name="rq"></param>
        /// <returns></returns>
        public override Task<RpcResult> OnRequest(IDataProtocol rq)
        {
            var more = rq as DefaultDataProtocol;

            switch ((EOps)more.Ops)
            {
                case EOps.Reqeust:
                    {
                        var proto = PbSerializer.Deserialize<RequestProto>(more.Data);
                        var ret = RpcResult.MakeSuccess(new ResponseProto
                        {
                            Message = $"Response from server : {proto.Message}"
                        });

                        return Task.FromResult(ret);
                    }

                default:
                    return Task.FromResult(RpcResult.Failure);
            }
        }

        public override async Task<bool> OnPush(IDataProtocol ps)
        {
            var more = ps as DefaultDataProtocol;
            switch ((EOps)more.Ops)
            {
                case EOps.Push:
                {
                    var proto = PbSerializer.Deserialize<PushProto>(more.Data);

                    // ¹ã²¥Àý×Ó
                    return await BroadcastAsync(new DefaultDataProtocol
                    {
                        Ops = (short) EOps.Push,
                        Data = PbSerializer.Serialize(new PushProto
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