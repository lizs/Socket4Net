using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using socket4net.BaseProto;
using socket4net.Log;
using socket4net.Net.TCP;
using socket4net.Serialize;

namespace socket4net.RPC
{
    public interface IRpcSession : ISession
    {
        Task<Tuple<bool, byte[]>> HandleRequest(ushort route, byte[] param);
        Task<bool> HandlePush(ushort route, byte[] param);

        Task<Tuple<bool, byte[]>> RequestAsync<T>(ushort route, T proto);
        Task<Tuple<bool, byte[]>> RequestAsync(ushort route, byte[] protoBytes);

        void Push<T>(ushort route, T proto);
        void Push(ushort route, byte[] protoBytes);

        void PushAll<T>(ushort route, T proto);
        void PushAll(ushort route, byte[] protoBytes);
    }

    public abstract class RpcSession : Session, IRpcSession
    {
        private readonly ConcurrentDictionary<ushort, Action<bool, byte[]>> _requestPool
            = new ConcurrentDictionary<ushort, Action<bool, byte[]>>();
        
        public override void Close(SessionCloseReason reason)
        {
            base.Close(reason);
            _requestPool.Clear();
        }

        public abstract Task<Tuple<bool, byte[]>> HandleRequest(ushort route, byte[] param);
        public abstract Task<bool> HandlePush(ushort route, byte[] param);

        public void Push<T>(ushort route, T proto)
        {
            var protoBytes = Serializer.Serialize(new RpcPush { Param = Serializer.Serialize(proto) });
            Push(route, protoBytes);
        }
        
        public void Push(ushort route, byte[] protoBytes)
        {
            SendWithHeader(PackRpc(RpcType.Push, route, protoBytes));
        }

        public void PushAll(ushort route, byte[] protoBytes)
        {
            BroadcastWithHeader(PackRpc(RpcType.Push, route, protoBytes));
        }

        public void PushAll<T>(ushort route, T proto)
        {
            var protoBytes = Serializer.Serialize(new RpcPush { Param = Serializer.Serialize(proto) });
            PushAll(route, protoBytes);
        }

        public async Task<Tuple<bool, byte[]>> RequestAsync(ushort route, byte[] protoBytes)
        {
            var tcs = new TaskCompletionSource<Tuple<bool, byte[]>>();

            Action<bool, byte[]> cb = (b, bytes) => tcs.SetResult(new Tuple<bool, byte[]>(b, bytes));
            if (!_requestPool.TryAdd(route, cb))
            {
                Logger.Instance.ErrorFormat("Route {0} already processing!", route);
                return new Tuple<bool, byte[]>(false, null);
            }

            SendWithHeader(PackRpc(RpcType.Request, route, protoBytes));
            return await tcs.Task;
        }

        public async Task<Tuple<bool, byte[]>> RequestAsync<T>(ushort route, T proto)
        {
            var protoBytes = Serializer.Serialize(new RpcReqeust() { Param = Serializer.Serialize(proto) });
            return await RequestAsync(route, protoBytes);
        }
        
        private static MemoryStream Extract(byte[] pack, out RpcType type, out ushort route)
        {
            var one = pack[0];
            var two = pack[1];

            var header = (ushort)(two << 8 | one);
            type = (RpcType)((header & 0xC000) >> 14);
            route = (ushort)(header & 0x3FFF);

            return new MemoryStream(pack, 2, pack.Length - 2);
        }

        private static ushort CreateHeader(RpcType type, ushort route)
        {
            return (ushort)((ushort)type << 14 | route);
        }

        public async override Task Dispatch(byte[] pack)
        {
            RpcType type;
            ushort route;
            using (var ms = Extract(pack, out type, out route))
            {
                switch (type)
                {
                    case RpcType.Request:
                        {
                            try
                            {
                                var rq = ProtoBuf.Serializer.Deserialize<RpcReqeust>(ms);
                                var rp = await HandleRequest(route, rq.Param);
                                if (rp == null)
                                    Response(route, null, false);
                                else
                                    Response(route, rp.Item2, rp.Item1);
                            }
                            catch (Exception e)
                            {
                                Logger.Instance.ErrorFormat("Exception {0} when processing route {1}", e.Message, route);
                                Response(route, null, false);
                            }
                        }
                        break;

                    case RpcType.Response:
                        {
                            try
                            {
                                var rp = ProtoBuf.Serializer.Deserialize<RpcResponse>(ms);

                                if (_requestPool.ContainsKey(route))
                                {
                                    var cb = _requestPool[route];

                                    Action<bool, byte[]> x;
                                    if (!_requestPool.TryRemove(route, out x))
                                        Logger.Instance.ErrorFormat("Remove response {0} failed", route);

                                    cb(rp.Success, rp.Param);
                                }
                                else
                                    Logger.Instance.ErrorFormat("No target for response {0}", route);
                            }
                            catch (Exception e)
                            {
                                Logger.Instance.ErrorFormat("Exception {0} when processing route {1}", e.Message, route);
                            }
                        }
                        break;

                    case RpcType.Push:
                        {
                            try
                            {
                                var notify = ProtoBuf.Serializer.Deserialize<RpcPush>(ms);

                                var success = await HandlePush(route, notify.Param);
                                if (!success)
                                    Logger.Instance.ErrorFormat("Handle push {0} failed!", route);
                            }
                            catch (Exception e)
                            {
                                Logger.Instance.ErrorFormat("Exception {0} when processing route {1}", e.Message, route);
                            }
                        }
                        break;

                    default:
                        Logger.Instance.ErrorFormat("Invalid rpc type : {0} of  route : {1}", type, route);
                        Close(SessionCloseReason.ClosedByMyself);
                        break;
                }
            }
        }

        private void Response(ushort route, byte[] protoBytes, bool success)
        {
            SendWithHeader(PackRpc(RpcType.Response, route, protoBytes, success));
        }

        private byte[] PackRpc(RpcType type, ushort route, byte[] data, bool success = true)
        {  
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Seek(2, SeekOrigin.Begin);
                bw.Write(CreateHeader(type, route));

                byte[] param;
                switch (type)
                {
                    case RpcType.Push:
                    case RpcType.Request:
                        param = data;
                        break;

                    case RpcType.Response:
                        param =
                            Serializer.Serialize(new RpcResponse()
                            {
                                Param = data,
                                Success = success
                            });
                        break;

                    default:
                        throw new ArgumentException("RpcType");
                }

                bw.Write(param);

                bw.Seek(0, SeekOrigin.Begin);
                bw.Write((ushort) (2 + param.Length));

                return ms.ToArray();
            }
        }
    }
}
