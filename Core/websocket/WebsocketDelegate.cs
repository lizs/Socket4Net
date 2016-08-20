
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
        void Close();
        void SendAsync(byte[] bytes, Action<bool> cb);
        void Send(byte[] bytes);
        Task<NetResult> OnRequest(IDataProtocol dp);
        Task<bool> OnPush(IDataProtocol dp);
    }

    /// <summary>
    ///     websocket delegate server host
    /// </summary>
    public interface IWebsocketDelegateServerHost : IWebsocketDelegateHost
    {
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
            GlobalVarPool.Ins.LogicService.Perform(async () =>
            {
                if (e.IsBinary)
                {
                    PerformanceMonitor.Ins.RecordRead(e.RawData.Length);
                    await OnMessage(e.RawData);
                }
                else if (e.IsText)
                {
                    PerformanceMonitor.Ins.RecordRead(e.Data.Length);
                    await OnMessage(e.Data);
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
        private async Task OnMessage(string data)
        {
        }

        /// <summary>
        ///     bytes handler
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private async Task OnMessage(byte[] data)
        {
            var pack = PiSerializer.Deserialize<NetPackage>(data);

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
        public async Task<NetResult> BroadcastAsync(byte[] data)
        {
            var serverHost = (IWebsocketDelegateServerHost) _host;
            if(serverHost == null)
                return NetResult.Failure;

            return await TaskHelper.WrapCallback<byte[], NetResult>(data, (bytes, cb) =>
            {
                serverHost.BroadcastAsync(bytes, () =>
                {
                    cb(NetResult.Success);
                });
            });
        }

        /// <summary>
        ///     asynchronous broadcast a proto
        /// </summary>
        /// <param name="proto"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<NetResult> BroadcastAsync<T>(T proto) where T : IDataProtocol
        {
            var data = PiSerializer.Serialize(PackPush(proto));
            return await BroadcastAsync(data);
        }

        /// <summary>
        ///     send bytes asynchronous
        ///     thread safe
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<NetResult> SendAsync(byte[] data)
        {
            return await TaskHelper.WrapCallback<byte[], NetResult>(data, (bytes, cb) =>
            {
                _host.SendAsync(bytes, complete =>
                {
                    if(complete)
                        PerformanceMonitor.Ins.RecordWrite(bytes.Length);

                    cb(complete ? NetResult.Success : NetResult.Failure);
                });
            });
        }

        /// <summary>
        ///     send a proto asynchronous
        /// </summary>
        /// <param name="proto"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<NetResult> SendAsync<T>(T proto) where T : IDataProtocol
        {
            var data = PiSerializer.Serialize(proto);
            return await SendAsync(data);
        }

        #endregion handlers

        /// <summary>
        ///     Get/Set custom data parser
        /// </summary>
        protected Func<byte[], IDataProtocol> DataParser { get; set; } = data => PiSerializer.Deserialize<DefaultDataProtocol>(data);
        
        /// <summary>
        /// 多播
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="proto"></param>
        /// <param name="sessions"></param>
        public void MultiCast<T>(T proto, IEnumerable<WebsocketSession> sessions) where T : IDataProtocol
        {
            var data = PiSerializer.Serialize(PackPush(proto));
            foreach (var session in sessions)
            {
                session.SendAsync(data);
            }
        }

        /// <summary>
        ///     request an data protocol asynchronous
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="proto"></param>
        /// <returns></returns>
        public async Task<NetResult> RequestAsync<T>(T proto) where T : IDataProtocol
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

            _host.Send(PiSerializer.Serialize(pack));
        }

        /// <summary>
        /// </summary>
        /// <param name="proto"></param>
        /// <typeparam name="T"></typeparam>
        public async Task<NetResult> Push<T>(T proto) where T : IDataProtocol
        {
            return await SendAsync(PiSerializer.Serialize(PackPush(proto)));
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
        private async Task<NetResult> RequestAsync(ushort serial, NetPackage pack)
        {
            var tcs = new TaskCompletionSource<NetResult>();

            //  call back in logic service
            Action<bool, byte[]> cb =
                (b, bytes) => GlobalVarPool.Ins.LogicService.Perform(() =>
                {
                    tcs.SetResult(new NetResult(b, bytes));
                });

            // callback pool
            if (!_requestPool.TryAdd(serial, cb))
            {
                Logger.Ins.Error("Request of serial {0} already processing!", serial);
                return NetResult.Failure;
            }

            // send
            var sendRet = await SendAsync(PiSerializer.Serialize(pack));
            if (!sendRet)
            {
                Action<bool, byte[]> _;
                _requestPool.TryRemove(serial, out _);
                _ = _ ?? cb;

                _(false, null);
            }

            return await tcs.Task;
        }

        private async Task<NetResult> Response(bool success, byte[] data, ushort serial)
        {
            var pack = new NetPackage
            {
                Type = ERpc.Response,
                Serial = serial,
                Success = success,
                Data = data,
            };

            return await SendAsync(PiSerializer.Serialize(pack));
        }
        private static NetPackage PackRequest<T>(T proto) where T : IDataProtocol
        {
            return new NetPackage { Type = ERpc.Request, Data = PiSerializer.Serialize(proto) };
        }
        private static NetPackage PackPush<T>(T proto) where T : IDataProtocol
        {
            return new NetPackage { Type = ERpc.Push, Data = PiSerializer.Serialize(proto) };
        }
    }
}
