using System;
#if NET45
using System.Threading.Tasks;
#endif

namespace socket4net
{
    public interface IRpcSession : ISession
    {
#if NET45
        Task<RpcResult> HandleRequest(RpcRequest rq);
        Task<bool> HandlePush(RpcPush rp);

        Task<RpcResult> RequestAsync<T>(byte targetServer, long playerId, short ops, T proto, long objId, short componentId) where T : IProtobufInstance;
        Task<RpcResult> RequestAsync(byte targetServer, long playerId, short ops, byte[] data, long objId, short componentId);
        Task<RpcResult> RequestAsync(byte targetServer, long playerId, short ops, long objId, short componentId);
#else
        RpcResult HandleRequest(RpcRequest rq);
        bool HandlePush(RpcPush rp);
#endif

        void RequestAsync<T>(byte targetServer, long playerId, short ops, T proto, long objId, short componentId,
            Action<bool, byte[]> cb)
            where T : IProtobufInstance;

        void RequestAsync(byte targetServer, long playerId, short ops, byte[] data, long objId, short componentId,
            Action<bool, byte[]> cb);

        void RequestAsync(byte targetServer, long playerId, short ops, long objId, short componentId,
            Action<bool, byte[]> cb);

        void Push<T>(byte targetServer, long playerId, short ops, T proto, long objId, short componentId)
            where T : IProtobufInstance;

        void Push(byte targetServer, long playerId, short ops, byte[] proto, long objId, short componentId);

        void Push<T>(long playerId, short ops, T proto, long objId, short componentId) where T : IProtobufInstance;
        void Push(long playerId, short ops, byte[] data, long objId, short componentId);
    }

    public abstract class RpcSession : Session, IRpcSession
    {
        private ushort _serial = 0;
        public const ushort RpcHeaderSize = sizeof (int);

        private readonly ConcurrentDictionary<ushort, Action<bool, byte[]>> _requestPool
            = new ConcurrentDictionary<ushort, Action<bool, byte[]>>();

        public override void Close(SessionCloseReason reason)
        {
            base.Close(reason);

            // 请求失败
            foreach (var kv in _requestPool)
            {
                kv.Value(false, null);
            }

            _requestPool.Clear();
        }

#if NET45
        public abstract Task<RpcResult> HandleRequest(RpcRequest rq);
        public abstract Task<bool> HandlePush(RpcPush rp);
#else
        public abstract RpcResult HandleRequest(RpcRequest rq);
        public abstract bool HandlePush(RpcPush rp);
#endif

#if NET45

        public async Task<RpcResult> RequestAsync(byte targetServer, long playerId, short ops, long objId, short componentId)
        {
            var rq = new RpcPack
            {
                Type = ERpc.Request,
                TargetNode = targetServer,
                PlayerId = playerId,
                Ops = ops,
                ObjId = objId,
                ComponentId = componentId,
                Serial = _serial++,
            };
            
            return await RequestAsync(rq.Serial, PiSerializer.Serialize(rq));
        }

        public async Task<RpcResult> RequestAsync<T>(byte targetServer, long playerId, short ops, T proto, long objId,
            short componentId)
            where T : IProtobufInstance
        {
            return await RequestAsync(targetServer, playerId, ops, PiSerializer.Serialize(proto), objId, componentId);
        }

        public async Task<RpcResult> RequestAsync(byte targetServer, long playerId, short ops, byte[] data, long objId, short componentId)
        {
            var rq = new RpcPack
            {
                Type = ERpc.Request,
                TargetNode = targetServer,
                PlayerId = playerId,
                Ops = ops,
                ObjId = objId,
                ComponentId = componentId,
                Serial = _serial++,
                Data = data,
            };

            return await RequestAsync(rq.Serial, PiSerializer.Serialize(rq));
        }

         /// <summary>
        ///     异步请求（按请求序列号回调结果）
        /// </summary>
        /// <param name="serial"></param>
        /// <param name="protoBytes"></param>
        /// <returns></returns>
        private async Task<RpcResult> RequestAsync(ushort serial, byte[] protoBytes)
        {
            var tcs = new TaskCompletionSource<RpcResult>();

            //  在逻辑服务线程回调
            Action<bool, byte[]> cb =
                (b, bytes) => HostPeer.PerformInLogic(() => tcs.SetResult(new RpcResult(b, bytes)));

            // 回调池
            if (!_requestPool.TryAdd(serial, cb))
            {
                Logger.Instance.ErrorFormat("Request of serial {0} already processing!", serial);
                return new RpcResult(false, null);
            }

            // 发送
            Send(protoBytes);

            return await tcs.Task;
        }
#endif

        public void RequestAsync<T>(byte targetServer, long playerId, short ops, T proto, long objId, short componentId, Action<bool, byte[]> cb)
            where T : IProtobufInstance
        {
            RequestAsync(targetServer, playerId, ops, PiSerializer.Serialize(proto), objId, componentId, cb);
        }

        public void RequestAsync(byte targetServer, long playerId, short ops, byte[] data, long objId, short componentId, Action<bool, byte[]> cb)
        {
            var rq = new RpcPack
            {
                Type = ERpc.Request,
                TargetNode = targetServer,
                PlayerId = playerId,
                Ops = ops,
                ObjId = objId,
                ComponentId = componentId,
                Serial = _serial++,
                Data = data,
            };

            // 回调池
            if (!_requestPool.TryAdd(rq.Serial, cb))
            {
                Logger.Instance.ErrorFormat("Request of serial {0} already processing!", rq.Serial);
                return;
            }

            Send(rq);
        }

        public void RequestAsync(byte targetServer, long playerId, short ops, long objId, short componentId, Action<bool, byte[]> cb)
        {
            var rq = new RpcPack
            {
                Type = ERpc.Request,
                TargetNode = targetServer,
                PlayerId = playerId,
                Ops = ops,
                ObjId = objId,
                ComponentId = componentId,
                Serial = _serial++,
            };

            // 回调池
            if (!_requestPool.TryAdd(rq.Serial, cb))
            {
                Logger.Instance.ErrorFormat("Request of serial {0} already processing!", rq.Serial);
                return;
            }

            Send(rq);
        }

        public void Push<T>(byte targetServer, long playerId, short ops, T proto, long objId, short componentId)
            where T : IProtobufInstance
        {
            Push(targetServer, playerId, ops, PiSerializer.Serialize(proto), objId, componentId);
        }

        public void Push(byte targetServer, long playerId, short ops, byte[] data, long objId, short componentId)
        {
            var ps = new RpcPack
            {
                Type = ERpc.Push,
                TargetNode = targetServer,
                PlayerId = playerId,
                Ops = ops,
                ObjId = objId,
                ComponentId = componentId,
                Data = data,
            };

            Send(PiSerializer.Serialize(ps));
        }

        public void Push<T>(long playerId, short ops, T proto, long objId, short componentId)
            where T : IProtobufInstance
        {
            Push(playerId, ops, PiSerializer.Serialize(proto), objId, componentId);
        }
        
        public void Push(long playerId, short ops, byte[] data, long objId, short componentId)
        {
            var ps = new RpcPack
            {
                Type = ERpc.Push,
                PlayerId = playerId,
                Ops = ops,
                ObjId = objId,
                ComponentId = componentId,
                Data = data,
            };

            Send(PiSerializer.Serialize(ps));
        }

#if NET35
        public override void Dispatch(byte[] packData)
#else
        public override async Task Dispatch(byte[] packData)
#endif
        {
            var pack = PiSerializer.Deserialize<RpcPack>(packData);

            switch (pack.Type)
            {
                case ERpc.Request:
                {
                    try
                    {
                        var rq = new RpcRequest
                        {
                            TargetNode = pack.TargetNode,
                            PlayerId = pack.PlayerId,
                            ObjId = pack.ObjId,
                            ComponentId = pack.ComponentId,
                            Ops = pack.Ops,
                            Data = pack.Data,
                        };
#if NET35
                        var rp = HandleRequest(rq);
#else
                        var rp = await HandleRequest(rq);
#endif
                        if (rp == null)
                            Response(false, null, pack.Serial);
                        else
                            Response(rp.Key, rp.Value, pack.Serial);
                    }
                    catch (Exception e)
                    {
                        Logger.Instance.ErrorFormat("Exception {0} : {1} when processing request {2}", e.Message,
                            e.StackTrace, pack);
                        Response(false, null, pack.Serial);
                    }
                    break;
                }

                case ERpc.Response:
                {
                    try
                    {
                        if (_requestPool.ContainsKey(pack.Serial))
                        {
                            var cb = _requestPool[pack.Serial];

                            Action<bool, byte[]> x;
                            if (!_requestPool.TryRemove(pack.Serial, out x))
                                Logger.Instance.ErrorFormat("Remove response of serial {0} failed", pack.Serial);

                            cb(pack.Succes, pack.Data);
                        }
                        else
                            Logger.Instance.ErrorFormat("No target for response of serial {0}", pack.Serial);
                    }
                    catch (Exception e)
                    {
                        Logger.Instance.ErrorFormat("Exception {0} : {1} when processing response {2}", e.Message,
                            e.StackTrace, pack);
                    }
                    break;
                }

                case ERpc.Push:
                {
                    try
                    {
                        var rp = new RpcPush
                        {
                            TargetNode = pack.TargetNode,
                            PlayerId = pack.PlayerId,
                            ObjId = pack.ObjId,
                            ComponentId = pack.ComponentId,
                            Ops = pack.Ops,
                            Data = pack.Data,
                        };
#if NET35
                        var success = HandlePush(rp);
#else
                        var success = await HandlePush(rp);
#endif
                        if (!success)
                            Logger.Instance.ErrorFormat("Handle push {0} failed!", rp.Ops);
                    }
                    catch (Exception e)
                    {
                        Logger.Instance.ErrorFormat("Exception {0} : {1} when processing push {2}", e.Message,
                            e.StackTrace, pack);
                    }
                }
                    break;

                default:
                    Logger.Instance.ErrorFormat("Invalid rpc type : {0} of  route : {1}", pack.Type, pack.Serial);
                    Close(SessionCloseReason.ClosedByMyself);
                    break;
            }
        }

        private void Response(bool success, byte[] data, ushort serial)
        {
            var package = new RpcPack
            {
                Type = ERpc.Response,
                Serial = serial,
                Succes = success,
                Data = data,
            };

            Send(PiSerializer.Serialize(package));
        }
    }
}
