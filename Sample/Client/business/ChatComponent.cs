using System.Threading.Tasks;
using Shared;
using socket4net;

namespace Sample
{
    public class SampleComponent : SampleComponentBase
    {
        /// <summary>
        ///     请求创建一个Ship
        /// </summary>
        /// <returns></returns>
        public async Task<RpcResult> Add()
        {
            var ret =
                await
                    RequestAsync(0, GetAncestor<Player>().Id, (short)EOps.Create, null, 0, Id);

            if (!ret)
                Logger.Ins.Error("Add failed!");

            return ret;
        }

        /// <summary>
        ///     请求删除所有Ship
        /// </summary>
        /// <returns></returns>
        public async Task<RpcResult> Del()
        {
            var ret =
                await
                    RequestAsync(0, GetAncestor<Player>().Id, (short)EOps.Destroy, null, 0, Id);

            if (!ret)
                Logger.Ins.Error("Add failed!");

            return ret;
        } 

        /// <summary>
        ///     请求将msg echo回来
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public async Task<bool> Echo(string msg)
        {
            var ret =
                await
                    RequestAsync(0, GetAncestor<Player>().Id, (short) EOps.Echo, new EchoProto {Message = msg},
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

        /// <summary>
        ///     请求将msg广播给所有客户端
        /// </summary>
        /// <param name="msg"></param>
        public void Broadcast(string msg)
        {
            Push(0, GetAncestor<Player>().Id, (short)EOps.Broadcst, new BroadcastProto { Message = msg },
                0, Id);
        }

        public async override Task<RpcResult> OnRequest(short ops, byte[] data)
        {
            switch ((EOps)ops)
            {
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
                        Logger.Ins.Info(proto.Message);
                        return true;
                    }

                default:
                    return false;
            }
        }
    }
}
