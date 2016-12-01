
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp;

namespace socket4net
{
    /// <summary>
    ///     websocket delegate host
    /// </summary>
    public interface IWebsocketDelegateHost
    {
        /// <summary>
        ///     close session
        /// </summary>
        void Close();

        /// <summary>
        ///     send async
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="cb"></param>
        void SendAsync(byte[] bytes, Action<bool> cb);

        /// <summary>
        /// </summary>
        /// <param name="bytes"></param>
        void Send(byte[] bytes);

        /// <summary>
        ///     request handler
        /// </summary>
        /// <param name="dp"></param>
        /// <returns></returns>
        Task<RpcResult> OnRequest(IDataProtocol dp);

        /// <summary>
        ///     push handler
        /// </summary>
        /// <param name="dp"></param>
        /// <returns></returns>
        Task<bool> OnPush(IDataProtocol dp);
    }

    /// <summary>
    ///     websocket delegate server host
    /// </summary>
    public interface IWebsocketDelegateServerHost : IWebsocketDelegateHost
    {
        /// <summary>
        ///     broadcast async
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="cb"></param>
        void BroadcastAsync(byte[] bytes, Action cb);
    }

    /// <summary>
    ///     websocket session delegate
    /// </summary>
    internal class WebsocketDelegate<THost> where THost : IWebsocketDelegateHost
    {
        private readonly THost _host;
        private int _serial;
        private readonly ConcurrentDictionary<ushort, Action<bool, byte[]>> _requestPool
            = new ConcurrentDictionary<ushort, Action<bool, byte[]>>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="host"></param>
        public WebsocketDelegate(THost host)
        {
            _host = host;
        }

        /// <summary>
        ///     Called when the <see cref="T:WebSocketSharp.WebSocket" /> used in the current session receives a message.
        /// </summary>
        /// <param name="e">
        ///     A <see cref="T:WebSocketSharp.MessageEventArgs" /> that represents the event data passed to
        ///     a <see cref="!:WebSocket.OnMessage" /> event.
        /// </param>
        public void OnMessage(MessageEventArgs e)
        {
            GlobalVarPool.Ins.Service.Perform(async () =>
            {
                if (e.IsBinary)
                {
                    PerformanceMonitor.Ins.RecordRead(e.RawData.Length);
                    await OnMessage(e.RawData);
                }
                else if (e.IsText)
                {
                    PerformanceMonitor.Ins.RecordRead(e.Data.Length);
                    OnMessage(e.Data);
                }
                else
                {
                    Logger.Ins.Error($"Unhandled message : {e}");
                }
            });
        }

        public void OnOpen()
        {
        }

        /// <summary>
        ///     Called when the <see cref="WebSocket" /> used in a session gets an error.
        /// </summary>
        /// <param name="e">
        ///     A <see cref="ErrorEventArgs" /> that represents the event data passed to
        ///     a <see cref="WebSocket.OnError" /> event.
        /// </param>
        public void OnError(ErrorEventArgs e)
        {
            Logger.Ins.Error(e.Message);
            if (e.Exception != null)
                Logger.Ins.Exception("OnError", e.Exception);
        }

        /// <summary>
        /// Called when the WebSocket connection used in a session has been closed.
        /// </summary>
        /// <param name="e">
        /// A <see cref="CloseEventArgs"/> that represents the event data passed to
        /// a <see cref="WebSocket.OnClose"/> event.
        /// </param>
        public void OnClose(CloseEventArgs e)
        {
            // make all requests failure
            foreach (var kv in _requestPool)
            {
                kv.Value(false, null);
            }

            _requestPool.Clear();
        }

        #region async message handlers

        /// <summary>
        ///     string handler
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private static void OnMessage(string data)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     bytes handler
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private async Task OnMessage(byte[] data)
        {
            var pack = PbSerializer.Deserialize<RpcPackage>(data);

            switch (pack.Type)
            {
                case ERpc.Request:
                    {
                        try
                        {
                            var rq = DataParser(pack.Data);
                            var rp = await _host.OnRequest(rq);
                            if (rp == null)
                                await Response(false, null, pack.Serial);
                            else
                                await Response(rp.Key, rp.Value, pack.Serial);
                        }
                        catch (Exception e)
                        {
                            Logger.Ins.Error("Exception {0} : {1} when processing request {2}", e.Message,
                                e.StackTrace, pack);
                            await Response(false, null, pack.Serial);
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
                                    Logger.Ins.Error("Remove response of serial {0} failed", pack.Serial);

                                cb(pack.Success, pack.Data);
                            }
                            else
                                Logger.Ins.Error("No target for response of serial {0}", pack.Serial);
                        }
                        catch (Exception e)
                        {
                            Logger.Ins.Error("Exception {0} : {1} when processing response {2}", e.Message,
                                e.StackTrace, pack);
                        }
                        break;
                    }

                case ERpc.Push:
                    {
                        try
                        {
                            var ps = DataParser(pack.Data);
                            var success = await _host.OnPush(ps);
                            if (!success)
                                Logger.Ins.Error("Handle push {0} failed!", ps);
                        }
                        catch (Exception e)
                        {
                            Logger.Ins.Error("Exception {0} : {1} when processing push {2}", e.Message,
                                e.StackTrace, pack);
                        }
                    }
                    break;

                default:
                    Logger.Ins.Error("Invalid rpc type : {0} of  route : {1}", pack.Type, pack.Serial);
                    _host.Close();
                    break;
            }
        }
            
        /// <summary>
        ///     asynchronous broadcast bytes
        ///     thread safe
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<RpcResult> BroadcastAsync(byte[] data)
        {
            var serverHost = (IWebsocketDelegateServerHost) _host;
            if(serverHost == null)
                return RpcResult.Failure;

            return await TaskHelper.WrapCallback<byte[], RpcResult>(data, (bytes, cb) =>
            {
                serverHost.BroadcastAsync(bytes, () =>
                {
                    cb(RpcResult.Success);
                });
            });
        }

        /// <summary>
        ///     asynchronous broadcast a proto
        /// </summary>
        /// <param name="proto"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<RpcResult> BroadcastAsync<T>(T proto) where T : IDataProtocol
        {
            var data = PbSerializer.Serialize(PackPush(proto));
            return await BroadcastAsync(data);
        }

        /// <summary>
        ///     send bytes asynchronous
        ///     thread safe
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<RpcResult> SendAsync(byte[] data)
        {
            return await TaskHelper.WrapCallback<byte[], RpcResult>(data, (bytes, cb) =>
            {
                _host.SendAsync(bytes, complete =>
                {
                    if(complete)
                        PerformanceMonitor.Ins.RecordWrite(bytes.Length);

                    cb(complete ? RpcResult.Success : RpcResult.Failure);
                });
            });
        }

        /// <summary>
        ///     send a proto asynchronous
        /// </summary>
        /// <param name="proto"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<RpcResult> SendAsync<T>(T proto) where T : IDataProtocol
        {
            var data = PbSerializer.Serialize(proto);
            return await SendAsync(data);
        }

        #endregion handlers

        /// <summary>
        ///     Get/Set custom data parser
        /// </summary>
        protected Func<byte[], IDataProtocol> DataParser { get; set; } = data => PbSerializer.Deserialize<DefaultDataProtocol>(data);
        
        /// <summary>
        ///     multicast
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="proto"></param>
        /// <param name="sessions"></param>
        public async void MultiCast<T>(T proto, IEnumerable<WebsocketSession> sessions) where T : IDataProtocol
        {
            var data = PbSerializer.Serialize(PackPush(proto));
            foreach (var session in sessions)
            {
                await session.SendAsync(data);
            }
        }

        /// <summary>
        ///     request an data protocol asynchronous
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="proto"></param>
        /// <returns></returns>
        public async Task<RpcResult> RequestAsync<T>(T proto) where T : IDataProtocol
        {
            var pack = PackRequest(proto);
            pack.Serial = GenerateSerial();

            return await RequestAsync(pack.Serial, pack);
        }

        /// <summary>
        /// </summary>
        /// <param name="proto"></param>
        /// <param name="cb"></param>
        /// <typeparam name="T"></typeparam>
        public void RequestAsync<T>(T proto, Action<bool, byte[]> cb) where T : IDataProtocol
        {
            var pack = PackRequest(proto);
            pack.Serial = GenerateSerial();

            if (!_requestPool.TryAdd(pack.Serial, cb))
            {
                Logger.Ins.Error("Request of serial {0} already processing!", pack.Serial);
                cb(false, null);
                return;
            }

            _host.Send(PbSerializer.Serialize(pack));
        }

        /// <summary>
        /// </summary>
        /// <param name="proto"></param>
        /// <typeparam name="T"></typeparam>
        public async Task<RpcResult> Push<T>(T proto) where T : IDataProtocol
        {
            return await SendAsync(PbSerializer.Serialize(PackPush(proto)));
        }

        private ushort GenerateSerial()
        {
            return (ushort)Interlocked.Increment(ref _serial);
        }

        /// <summary>
        ///     request asynchronous
        ///     ordered by "serial"
        /// </summary>
        /// <param name="serial"></param>
        /// <param name="pack"></param>
        /// <returns></returns>
        private async Task<RpcResult> RequestAsync(ushort serial, RpcPackage pack)
        {
            var tcs = new TaskCompletionSource<RpcResult>();

            //  call back in logic service
            Action<bool, byte[]> cb =
                (b, bytes) => GlobalVarPool.Ins.Service.Perform(() =>
                {
                    tcs.SetResult(new RpcResult(b, bytes));
                });

            // callback pool
            if (!_requestPool.TryAdd(serial, cb))
            {
                Logger.Ins.Error("Request of serial {0} already processing!", serial);
                return RpcResult.Failure;
            }

            // send
            var sendRet = await SendAsync(PbSerializer.Serialize(pack));
            if (sendRet) return await tcs.Task;

            Action<bool, byte[]> _;
            _requestPool.TryRemove(serial, out _);
            _ = _ ?? cb;
            _(false, null);
            return await tcs.Task;
        }

        private async Task<RpcResult> Response(bool success, byte[] data, ushort serial)
        {
            var pack = new RpcPackage
            {
                Type = ERpc.Response,
                Serial = serial,
                Success = success,
                Data = data,
            };

            return await SendAsync(PbSerializer.Serialize(pack));
        }
        private static RpcPackage PackRequest<T>(T proto) where T : IDataProtocol
        {
            return new RpcPackage { Type = ERpc.Request, Data = PbSerializer.Serialize(proto) };
        }
        private static RpcPackage PackPush<T>(T proto) where T : IDataProtocol
        {
            return new RpcPackage { Type = ERpc.Push, Data = PbSerializer.Serialize(proto) };
        }
    }
}
