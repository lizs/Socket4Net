using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using Core.BaseProto;
using Core.Log;
using Core.Net.TCP;
using Core.Serialize;

namespace Core.RPC
{
    public interface IRpcSession : ISession
    {
        Task<Tuple<bool, byte[]>> HandleRequest(short route, byte[] param);
        Task<bool> HandlePush(short route, byte[] param);

        Task<Tuple<bool, byte[]>> RequestAsync<T>(short route, T proto);
        Task<Tuple<bool, byte[]>> RequestAsync(short route, byte[] protoBytes);

        void Push<T>(short route, T proto);
        void Push(short route, byte[] protoBytes);

        void PushAll<T>(short route, T proto);
        void PushAll(short route, byte[] protoBytes);
    }

    public abstract class RpcSession : Session, IRpcSession
    {
        private readonly ConcurrentDictionary<short, Action<bool, byte[]>> _requestPool
            = new ConcurrentDictionary<short, Action<bool, byte[]>>();
        
        public override void Close(SessionCloseReason reason)
        {
            base.Close(reason);
            _requestPool.Clear();
        }

        public abstract Task<Tuple<bool, byte[]>> HandleRequest(short route, byte[] param);
        public abstract Task<bool> HandlePush(short route, byte[] param);

        public void Push<T>(short route, T proto)
        {
            var protoBytes = Serializer.Serialize(new RpcPush { Param = Serializer.Serialize(proto) });
            Push(route, protoBytes);
        }
        
        public void Push(short route, byte[] protoBytes)
        {
            SendWithHeader(PackRpc(RpcType.Push, route, protoBytes));
        }

        public void PushAll(short route, byte[] protoBytes)
        {
            BroadcastWithHeader(PackRpc(RpcType.Push, route, protoBytes));
        }

        public void PushAll<T>(short route, T proto)
        {
            var protoBytes = Serializer.Serialize(new RpcPush { Param = Serializer.Serialize(proto) });
            PushAll(route, protoBytes);
        }

        public async Task<Tuple<bool, byte[]>> RequestAsync(short route, byte[] protoBytes)
        {
            var tcs = new TaskCompletionSource<Tuple<bool, byte[]>>();

            Action<bool, byte[]> cb = (b, bytes) => tcs.TrySetResult(new Tuple<bool, byte[]>(b, bytes));
            if (!_requestPool.TryAdd(route, cb))
                tcs.TrySetException(new ArgumentOutOfRangeException("Route already exists!"));

            SendWithHeader(PackRpc(RpcType.Request, route, protoBytes));

            return await tcs.Task;
        }

        public async Task<Tuple<bool, byte[]>> RequestAsync<T>(short route, T proto)
        {
            var protoBytes = Serializer.Serialize(new RpcReqeust() { Param = Serializer.Serialize(proto) });
            return await RequestAsync(route, protoBytes);
        }
        
        private static MemoryStream Extract(byte[] pack, out RpcType type, out short route)
        {
            var one = pack[0];
            var two = pack[1];

            var header = (short)(two << 8 | one);
            type = (RpcType)((header & 0xC000) >> 14);
            route = (short)(header & 0x3FFF);

            return new MemoryStream(pack, 2, pack.Length - 2);
        }

        private static short CreateHeader(RpcType type, short route)
        {
            return (short)((short)type << 14 | route);
        }

        public async override Task Dispatch(byte[] pack)
        {
            RpcType type;
            short route;
            using (var ms = Extract(pack, out type, out route))
            {
                switch (type)
                {
                    case RpcType.Request:
                        {
                            var rq = ProtoBuf.Serializer.Deserialize<RpcReqeust>(ms);
                            var rp = await HandleRequest(route, rq.Param);
                            if(rp == null)
                                Response(route, null, false);
                            else
                                Response(route, rp.Item2, rp.Item1);
                        }
                        break;

                    case RpcType.Response:
                        {
                            var rp = ProtoBuf.Serializer.Deserialize<RpcResponse>(ms);

                            if (_requestPool.ContainsKey(route))
                            {
                                var cb = _requestPool[route];

                                Action<bool, byte[]> x;
                                _requestPool.TryRemove(route, out x);

                                cb(rp.Success, rp.Param);
                            }
                            else
                                Logger.Instance.ErrorFormat("No target for response {0}", route);
                        }
                        break;

                    case RpcType.Push:
                        {
                            var notify = ProtoBuf.Serializer.Deserialize<RpcPush>(ms);

                            var success = await HandlePush(route, notify.Param);
                            if (!success)
                                Logger.Instance.ErrorFormat("Handle notify {0} failed!", route);
                        }
                        break;

                    default:
                        Logger.Instance.ErrorFormat("Invalid rpc type : {0} of  route : {1}", type, route);
                        break;
                }
            }
        }

        private void Response(short route, byte[] protoBytes, bool success)
        {
            SendWithHeader(PackRpc(RpcType.Response, route, protoBytes, success));
        }

        private byte[] PackRpc(RpcType type, short route, byte[] data, bool success = true)
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
                bw.Write((short) (2 + param.Length));

                return ms.ToArray();
            }
        }
    }
}
