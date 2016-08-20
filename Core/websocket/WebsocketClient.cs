using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebSocketSharp;

namespace socket4net
{
    /// <summary>
    ///     websocket client
    /// </summary>
    public class WebsocketClient : IWebsocketDelegateHost
    {
        private WebsocketDelegate<WebsocketClient> _delegate;
        private WebsocketDelegate<WebsocketClient> SessionDelegate => _delegate ?? (_delegate = new WebsocketDelegate<WebsocketClient>(this));
        
        /// <summary>
        /// Occurs when the WebSocket connection has been closed.
        /// </summary>
        public event EventHandler<CloseEventArgs> OnClose;

        /// <summary>
        /// Occurs when the <see cref="WebSocket"/> gets an error.
        /// </summary>
        public event EventHandler<ErrorEventArgs> OnError;

        /// <summary>
        /// Occurs when the <see cref="WebSocket"/> receives a message.
        /// </summary>
        public event EventHandler<MessageEventArgs> OnMessage;

        /// <summary>
        /// Occurs when the WebSocket connection has been established.
        /// </summary>
        public event EventHandler OnOpen;

        /// <summary>
        ///     initialize client with a websocket
        /// </summary>
        /// <param name="ws"></param>
        public WebsocketClient(WebSocket ws)
        {
            _websocket = ws;
            _websocket.OnOpen += onOpen;
            _websocket.OnMessage += onMessage;
            _websocket.OnError += onError;
            _websocket.OnClose += onClose;
        }

        private void onClose(object sender, CloseEventArgs e)
        {
            SessionDelegate.OnClose(e);
            OnClose?.Invoke(sender, e);
        }

        private void onError(object sender, ErrorEventArgs e)
        {
            SessionDelegate.OnError(e);
            OnError?.Invoke(sender, e);
        }

        public void onMessage(object sender, MessageEventArgs e)
        {
            SessionDelegate.OnMessage(e);
            OnMessage?.Invoke(sender, e);
        }

        private void onOpen(object sender, EventArgs e)
        {
            SessionDelegate.OnOpen();
            OnOpen?.Invoke(sender, e);
        }

        /// <summary>
        ///  underline websocket
        /// </summary>
        private readonly WebSocket _websocket;

        public void Close()
        {
            _websocket?.Close();
        }

        public void SendAsync(byte[] bytes, Action<bool> cb)
        {
            _websocket?.SendAsync(bytes, cb);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        public void Send(byte[] bytes)
        {
            _websocket?.Send(bytes);
        }

        public void ConnectAsync()
        {
            _websocket?.ConnectAsync();
        }


        /// <summary>
        ///     handle request
        /// </summary>
        /// <param name="rq"></param>
        /// <returns></returns>
        protected virtual Task<NetResult> OnRequest(IDataProtocol rq)
        {
            return Task.FromResult(NetResult.Failure);
        }

        /// <summary>
        ///     handle push
        /// </summary>
        /// <param name="ps"></param>
        /// <returns></returns>
        protected virtual Task<bool> OnPush(IDataProtocol ps)
        {
            return Task.FromResult(false);
        }

        /// <summary>
        ///     request an data protocol asynchronous
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="proto"></param>
        /// <returns></returns>
        public async Task<NetResult> RequestAsync<T>(T proto) where T : IDataProtocol
        {
            return await SessionDelegate.RequestAsync(proto);
        }

        /// <summary>
        /// </summary>
        /// <param name="proto"></param>
        /// <param name="cb"></param>
        /// <typeparam name="T"></typeparam>
        public void RequestAsync<T>(T proto, Action<bool, byte[]> cb) where T : IDataProtocol
        {
            SessionDelegate.RequestAsync(proto, cb);
        }

        /// <summary>
        /// </summary>
        /// <param name="proto"></param>
        /// <typeparam name="T"></typeparam>
        public async Task<NetResult> Push<T>(T proto) where T : IDataProtocol
        {
            return await SessionDelegate.Push(proto);
        }
    }
}
