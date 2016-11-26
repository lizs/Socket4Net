using System;
using System.Threading.Tasks;
using WebSocketSharp;

namespace socket4net
{
    /// <summary>
    /// 
    /// </summary>
    public class WebsocketClientArg : UniqueObjArg<string>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="key"></param>
        /// <param name="url"></param>
        public WebsocketClientArg(IObj owner, string key, string url) : base(owner, key)
        {
            Url = url;
        }

        /// <summary>
        /// 
        /// </summary>
        public string Url { get; }
    }

    /// <summary>
    ///     websocket client
    /// </summary>
    public class WebsocketClient : UniqueObj<string>, IWebsocketDelegateHost
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
        ///    internal called when an Obj is initialized
        /// </summary>
        /// <param name="arg"></param>
        protected override void OnInit(ObjArg arg)
        {
            base.OnInit(arg);

            var more = arg as WebsocketClientArg;
            _websocket = new WebSocket(more?.Url);
            _websocket.OnOpen += OpenHandler;
            _websocket.OnMessage += MessageHandler;
            _websocket.OnError += ErrorHandler;
            _websocket.OnClose += CloseHandler;
        }


        private void CloseHandler(object sender, CloseEventArgs e)
        {
            SessionDelegate.OnClose(e);
            OnClose?.Invoke(sender, e);
        }

        private void ErrorHandler(object sender, ErrorEventArgs e)
        {
            SessionDelegate.OnError(e);
            OnError?.Invoke(sender, e);
        }

        /// <summary>
        ///     
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MessageHandler(object sender, MessageEventArgs e)
        {
            SessionDelegate.OnMessage(e);
            OnMessage?.Invoke(sender, e);
        }

        private void OpenHandler(object sender, EventArgs e)
        {
            SessionDelegate.OnOpen();
            OnOpen?.Invoke(sender, e);
        }

        /// <summary>
        ///  underline websocket
        /// </summary>
        private WebSocket _websocket;

        /// <summary>
        ///     specify if connection is alive
        /// </summary>
        public bool Connected => _websocket != null && _websocket.IsAlive;

        /// <summary>
        ///    internal called when an Obj is to be destroyed
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();
            Close();
        }

        /// <summary>
        ///     close session
        /// </summary>
        public void Close()
        {
            _websocket?.Close();
            _websocket = null;
        }

        /// <summary>
        ///     send async
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="cb"></param>
        public void SendAsync(byte[] bytes, Action<bool> cb)
        {
            _websocket?.SendAsync(bytes, b =>
            {
                cb?.Invoke(b);
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        public void Send(byte[] bytes)
        {
            _websocket?.Send(bytes);
        }

        /// <summary>
        ///     
        /// </summary>
        public void ConnectAsync()
        {
            _websocket?.ConnectAsync();
        }

        /// <summary>
        ///     handle request
        /// </summary>
        /// <param name="rq"></param>
        /// <returns></returns>
        public virtual Task<RpcResult> OnRequest(IDataProtocol rq)
        {
            return Task.FromResult(RpcResult.Failure);
        }

        /// <summary>
        ///     handle push
        /// </summary>
        /// <param name="ps"></param>
        /// <returns></returns>
        public virtual Task<bool> OnPush(IDataProtocol ps)
        {
            return Task.FromResult(false);
        }

        /// <summary>
        ///     request an data protocol asynchronous
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="proto"></param>
        /// <returns></returns>
        public async Task<RpcResult> RequestAsync<T>(T proto) where T : IDataProtocol
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
        public async Task<RpcResult> Push<T>(T proto) where T : IDataProtocol
        {
            return await SessionDelegate.Push(proto);
        }
    }
}
    