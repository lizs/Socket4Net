using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace socket4net
{
    /// <summary>
    ///     websocket session abstraction
    /// </summary>
    public abstract class WebsocketSession : WebSocketBehavior, IWebsocketDelegateServerHost
    {
        private WebsocketDelegate<WebsocketSession> _delegate;
        private WebsocketDelegate<WebsocketSession> SessionDelegate => _delegate ?? (_delegate = new WebsocketDelegate<WebsocketSession>(this));

        /// <summary>
        ///     Called when the <see cref="T:WebSocketSharp.WebSocket" /> used in the current session receives a message.
        /// </summary>
        /// <param name="e">
        ///     A <see cref="T:WebSocketSharp.MessageEventArgs" /> that represents the event data passed to
        ///     a <see cref="!:WebSocket.OnMessage" /> event.
        /// </param>
        protected sealed override void OnMessage(MessageEventArgs e)
        {
            base.OnMessage(e);
            SessionDelegate.OnMessage(e);
        }
        
        /// <summary>
        ///     Called when the <see cref="WebSocket" /> used in a session gets an error.
        /// </summary>
        /// <param name="e">
        ///     A <see cref="ErrorEventArgs" /> that represents the event data passed to
        ///     a <see cref="WebSocket.OnError" /> event.
        /// </param>
        protected override void OnError(ErrorEventArgs e)
        {
            base.OnError(e);
            SessionDelegate.OnError(e);
        }

        /// <summary>
        /// Called when the WebSocket connection used in a session has been established.
        /// </summary>
        protected override void OnOpen()
        {
            base.OnOpen();
            SessionDelegate.OnOpen();
        }

        /// <summary>
        /// Called when the WebSocket connection used in a session has been closed.
        /// </summary>
        /// <param name="e">
        /// A <see cref="CloseEventArgs"/> that represents the event data passed to
        /// a <see cref="WebSocket.OnClose"/> event.
        /// </param>
        protected override void OnClose(CloseEventArgs e)
        {
            base.OnClose(e);
            SessionDelegate.OnClose(e);
        }
        
        /// <summary>
        ///     string handler
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private Task<NetResult> OnMessage(string data)
        {
            return Task.FromResult(NetResult.Failure);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="cb"></param>
        public void BroadcastAsync(byte[] data, Action cb)
        {
            Sessions.BroadcastAsync(data, () =>
            {
                var sessionsCnt = Sessions.Count;
                PerformanceMonitor.Ins.RecordWrite(sessionsCnt * data.Length, sessionsCnt);
                cb();
            });
        }
        
        /// <summary>
        ///     asynchronous broadcast bytes
        ///     thread safe
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<NetResult> BroadcastAsync(byte[] data)
        {
            return await SessionDelegate.BroadcastAsync(data);
        }

        /// <summary>
        ///     asynchronous broadcast a proto
        /// </summary>
        /// <param name="proto"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<NetResult> BroadcastAsync<T>(T proto) where T : IDataProtocol
        {
            return await SessionDelegate.BroadcastAsync(proto);
        }

        /// <summary>
        ///     send bytes asynchronous
        ///     thread safe
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<NetResult> SendAsync(byte[] data)
        {
            return await SessionDelegate.SendAsync(data);
        }

        /// <summary>
        ///     explicit implementation
        /// </summary>
        /// <param name="data"></param>
        /// <param name="cb"></param>
        void IWebsocketDelegateHost.SendAsync(byte[] data, Action<bool> cb)
        {
            SendAsync(data, cb);
        }

        /// <summary>
        ///     explicit implementation
        /// </summary>
        /// <param name="data"></param>
        void IWebsocketDelegateHost.Send(byte[] data)
        {
            Send(data);
        }

        void IWebsocketDelegateHost.Close()
        {
            Context.WebSocket.Close();
        }

        /// <summary>
        ///     send a proto asynchronous
        /// </summary>
        /// <param name="proto"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
//        public async Task<NetResult> SendAsync<T>(T proto) where T : IDataProtocol
//        {
//            return await SessionDelegate.SendAsync(proto);
//        }
        
        /// <summary>
        ///     handle request
        /// </summary>
        /// <param name="rq"></param>
        /// <returns></returns>
        public virtual Task<NetResult> OnRequest(IDataProtocol rq)
        {
            return Task.FromResult(NetResult.Failure);
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
        ///     multicast
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="proto"></param>
        /// <param name="sessions"></param>
        public void MultiCast<T>(T proto, IEnumerable<WebsocketSession> sessions) where T : IDataProtocol
        {
            SessionDelegate.MultiCast(proto, sessions);
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
