using System;
using System.Collections.Generic;
using System.IO;
using Core.BaseProto;
using Core.Net.TCP;
using Core.Serialize;

namespace Core.RPC
{
    public class RpcSession : Session
    {
        private static readonly log4net.ILog Log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public readonly RpcHandlers Handlers = new RpcHandlers();

        private readonly Dictionary<RpcRoute, Action<bool, byte[]>> _requestPool = new Dictionary<RpcRoute, Action<bool, byte[]>>();

        private static MemoryStream Extract(byte[] pack, out RpcType type, out RpcRoute route)
        {
            var one = pack[0];
            var two = pack[1];

            var header = (short)(two << 8 | one);
            type = (RpcType)((header & 0xC000) >> 14);
            route = (RpcRoute)(header & 0x3FFF);

            return new MemoryStream(pack, 2, pack.Length - 2);
        }

        public override void Close(SessionCloseReason reason)
        {
            base.Close(reason);

            Handlers.Dispose();
            _requestPool.Clear();
        }

        private static short CreateHeader(RpcType type, RpcRoute route)
        {
            return (short)((short)type << 14 | (short)route);
        }

        public override void Dispatch(byte[] pack)
        {
            RpcType type;
            RpcRoute route;
            using (var ms = Extract(pack, out type, out route))
            {
                switch (type)
                {
                    case RpcType.Request:
                        {
                            var rq = ProtoBuf.Serializer.Deserialize<RpcReqeust>(ms);
                            var rp = Handlers.HandleRequest(route, rq.Param);
                            Response(route, rp, rp != null);
                        }
                        break;

                    case RpcType.Response:
                        {
                            var rp = ProtoBuf.Serializer.Deserialize<RpcResponse>(ms);

                            if (_requestPool.ContainsKey(route))
                            {
                                var cb = _requestPool[route];
                                _requestPool.Remove(route);

                                cb(rp.Success, rp.Param);
                            }
                            else
                                Log.ErrorFormat("No target for response {0}", route);
                        }
                        break;

                    case RpcType.Notify:
                        {
                            var notify = ProtoBuf.Serializer.Deserialize<RpcNotify>(ms);
                            if (!Handlers.HandleNotify(route, notify.Param))
                                Log.ErrorFormat("Handle notify {0} failed!", route);
                        }
                        break;

                    default:
                        Log.ErrorFormat("Invalid rpc type : {0} of  route : {1}", type, route);
                        break;
                }
            }
        }

        public void Response(RpcRoute route, object proto, bool success)
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Seek(2, SeekOrigin.Begin);
                bw.Write(CreateHeader(RpcType.Response, route));

                var rp = new RpcResponse { Param = Serializer.Serialize(proto), Success = success };
                var param = Serializer.Serialize(rp);
                bw.Write(param);

                bw.Seek(0, SeekOrigin.Begin);
                bw.Write((short)(2 + param.Length));

                SendWithHeader(ms.ToArray());
            }
        }

        public void Request(RpcRoute route, object proto, Action<bool, byte[]> cb)
        {
            if (_requestPool.ContainsKey(route))
            {
                Log.ErrorFormat("Previous request not response, ignore this request!");
                return;
            }

            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Seek(2, SeekOrigin.Begin);
                bw.Write(CreateHeader(RpcType.Request, route));

                var rq = new RpcReqeust() { Param = Serializer.Serialize(proto) };
                var param = Serializer.Serialize(rq);
                bw.Write(param);

                bw.Seek(0, SeekOrigin.Begin);
                bw.Write((short)(2 + param.Length));

                SendWithHeader(ms.ToArray());
            }

            _requestPool.Add(route, cb);
        }

        public void Notify(RpcRoute route, object proto)
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Seek(2, SeekOrigin.Begin);
                bw.Write(CreateHeader(RpcType.Notify, route));

                var notify = new RpcNotify() { Param = Serializer.Serialize(proto) };
                var param = Serializer.Serialize(notify);
                bw.Write(param);

                bw.Seek(0, SeekOrigin.Begin);
                bw.Write((short)(2 + param.Length));

                SendWithHeader(ms.ToArray());
            }
        }

        public static void NotifyAll(RpcRoute route, object proto)
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Seek(2, SeekOrigin.Begin);
                bw.Write(CreateHeader(RpcType.Notify, route));

                var notify = new RpcNotify() { Param = Serializer.Serialize(proto) };
                var param = Serializer.Serialize(notify);
                bw.Write(param);

                bw.Seek(0, SeekOrigin.Begin);
                bw.Write((short)(2 + param.Length));

                BroadcastWithHeader(ms.ToArray());
            }
        }
    }
}
