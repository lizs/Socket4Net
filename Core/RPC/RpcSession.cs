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
        object HandleRequest(short route, byte[] param);
        bool HandleNotify(short route, byte[] param);
        Task<Tuple<bool, byte[]>> Request(short route, object proto);
        void Notify(short route, object proto);
    }

    public abstract class RpcSession : Session, IRpcSession
    {
        private readonly ConcurrentDictionary<short, Action<bool, byte[]>> _requestPool = new ConcurrentDictionary<short, Action<bool, byte[]>>();
        
        public override void Close(SessionCloseReason reason)
        {
            base.Close(reason);

            _requestPool.Clear();
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

        public override void Dispatch(byte[] pack)
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
                            //var rp = Handlers.HandleRequest(route, rq.Param);
                            var rp = HandleRequest(route, rq.Param);
                            Response(route, rp, rp != null);
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

                    case RpcType.Notify:
                        {
                            var notify = ProtoBuf.Serializer.Deserialize<RpcNotify>(ms);
                            //if (!Handlers.HandleNotify(route, notify.Param))
                            if(!HandleNotify(route, notify.Param))
                                Logger.Instance.ErrorFormat("Handle notify {0} failed!", route);
                        }
                        break;

                    default:
                        Logger.Instance.ErrorFormat("Invalid rpc type : {0} of  route : {1}", type, route);
                        break;
                }
            }
        }

        private void Response(short route, object proto, bool success)
        {
            SendWithHeader(PackRpc(RpcType.Response, route, proto, success));
        }

        public async Task<Tuple<bool, byte[]>> Request(short route, object proto)
        {
            return await _Request(route, proto);
        }

        private Task<Tuple<bool, byte[]>> _Request(short route, object proto)
        {
            var tcs = new TaskCompletionSource<Tuple<bool, byte[]>>();

            Action<bool, byte[]> cb = (b, bytes) => tcs.TrySetResult(new Tuple<bool, byte[]>(b, bytes));
            if (!_requestPool.TryAdd(route, cb))
                tcs.TrySetException(new ArgumentOutOfRangeException("Route already exists!"));

            SendWithHeader(PackRpc(RpcType.Request, route, proto));
            
            return tcs.Task;
        }

        public void Notify(short route, object proto)
        {
            SendWithHeader(PackRpc(RpcType.Notify, route, proto));
        }

        public void NotifyAll(short route, object proto)
        {
            BroadcastWithHeader(PackRpc(RpcType.Notify, route, proto));
        }

        private byte[] PackRpc(RpcType type, short route, object proto, bool success = true)
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Seek(2, SeekOrigin.Begin);
                bw.Write(CreateHeader(type, route));

                byte[] param;
                switch (type)
                {
                    case RpcType.Notify:
                        param = Serializer.Serialize(new RpcNotify() { Param = Serializer.Serialize(proto) });
                        break;

                    case RpcType.Request:
                        param = Serializer.Serialize(new RpcReqeust() { Param = Serializer.Serialize(proto) });
                        break;

                    case RpcType.Response:
                        param = Serializer.Serialize(new RpcResponse() { Param = Serializer.Serialize(proto), Success = success});
                        break;

                    default:
                        throw new ArgumentException("RpcType");
                }

                bw.Write(param);

                bw.Seek(0, SeekOrigin.Begin);
                bw.Write((short)(2 + param.Length));

                return ms.ToArray();
            }
        }

        public abstract object HandleRequest(short route, byte[] param);
        public abstract bool HandleNotify(short route, byte[] param);
    }
}
