#region MIT
//  /*The MIT License (MIT)
// 
//  Copyright 2016 lizs lizs4ever@163.com
//  
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//  
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
//  
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
//   * */
#endregion
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

#if NET45

#endif

namespace socket4net
{
    public interface IClient : IPeer
    {
        void Reconnect();
        void Connect();
        bool Connected { get; }
    }
    
    public class ClientArg : PeerArg
    {
        public ClientArg(IObj parent, string ip, ushort port, bool autoReconnectEnabled = false)
            : base(parent, ip, port)
        {
            AutoReconnectEnabled = autoReconnectEnabled;
        }

        public bool AutoReconnectEnabled { get; private set; }
    }

    public class Client<TSession> : Obj, IClient
        where TSession : class, ISession, new()
    {
        public const uint ReconnectDefaultDelay = 1000;
        public const uint ReconnectMaxDelay = 32 * 1000;
        public event Action<ISession, SessionCloseReason> EventSessionClosed;
        public event Action<ISession> EventSessionEstablished;
        public event Action EventPeerClosing;
        public event Action<string> EventErrorCatched;

        public override string Name
        {
            get { return string.Format("{0}:{1}", Ip, Port); }
        }

        public string Ip { get; private set; }
        public ushort Port { get; private set; }
        public IPAddress Address { get; private set; }
        public EndPoint EndPoint { get; private set; }
        public SessionMgr SessionMgr { get; private set; }

        public ILogicService LogicService
        {
            get { return Launcher.Ins.LogicService; }
        }

        public INetService NetService
        {
            get { return Launcher.Ins.NetService; }
        }

        public bool Connected
        {
            get { return _connected; }
            private set
            {
                _connected = value;
                ReconnectRetryDelay = _connected
                    ? ReconnectDefaultDelay
                    : Math.Min(ReconnectMaxDelay, ReconnectRetryDelay*2);
            }
        }

        /// <summary>
        ///     是否在断开连接之后自动重连
        /// </summary>
        public bool AutoReconnectEnabled { get; set; }

        /// <summary>
        ///     重连重试延时
        /// </summary>
        private uint ReconnectRetryDelay
        {
            get { return _reconnectRetryDelay; }
            set { _reconnectRetryDelay = value; }
        }

        public TSession Session
        {
            get
            {
                var ret = SessionMgr.FirstOrDefault();
                return ret != null ? ret as TSession : null;
            }
        }

        private Socket _underlineSocket;
        private SocketAsyncEventArgs _connectEvent;
        private bool _connected;
        private uint _reconnectRetryDelay = ReconnectDefaultDelay;

        protected override void OnInit(ObjArg arg)
        {
            base.OnInit(arg);

            var more = arg.As<ClientArg>();
            AutoReconnectEnabled = more.AutoReconnectEnabled;

            if(string.IsNullOrEmpty(more.Ip) || more.Port == 0)
                Logger.Ins.Warn("Ip or Port is invalid!");
            else if (!SetAddress(more.Ip, more.Port))
                throw new Exception("Ip or Port is invalid!");

            SessionMgr = New<SessionMgr>(new SessionMgrArg(this,
                session =>
                {
                    OnConnected(session);

                    if (EventSessionEstablished != null)
                        EventSessionEstablished(session as TSession);
                },
                (session, reason) =>
                {
                    OnDisconnected(session, reason);

                    if (EventSessionClosed != null)
                        EventSessionClosed(session as TSession, reason);
                }));
        }

        protected virtual void OnConnected(ISession session)
        {
            Connected = true;
            Logger.Ins.Info("{0}:{1} connected!", Name, session.Name);
        }

        protected virtual void OnDisconnected(ISession session, SessionCloseReason reason)
        {
            Connected = false;
            Logger.Ins.Info("{0}:{1} disconnected by {2}", Name, session.Name, reason);

            if (AutoReconnectEnabled)
            {
                Invoke(Reconnect, ReconnectRetryDelay);
            }
        }

        protected virtual void OnError(string msg)
        {
            Connected = false;
            Logger.Ins.Error("{0}:{1}", Name, msg);

            if (AutoReconnectEnabled)
            {
                Invoke(Reconnect, ReconnectRetryDelay);
            }
        }

        public bool SetAddress(string ip, ushort port)
        {
            Ip = ip;
            Port = port;

            try
            {
                Address = IPAddress.Parse(Ip);
                EndPoint = new IPEndPoint(Address, Port);
            }
            catch (Exception e)
            {
                var msg = string.Format("{0} : {1}", e.Message, e.StackTrace);
                OnError(msg);

                if (EventErrorCatched != null)
                    EventErrorCatched(msg);

                Ip = string.Empty;
                Port = 0;
                return false;
            }

            return true;
        }

        protected override void OnStart()
        {
            base.OnStart();

            if(string.IsNullOrEmpty(Ip) || Port == 0)
                throw new Exception("Address must be setted before start!");

            _connectEvent = new SocketAsyncEventArgs();
            _connectEvent.Completed += OnConnectCompleted;

            _underlineSocket = SocketExt.CreateTcpSocket();

            Connect();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventSessionClosed = null;
            EventSessionEstablished = null;
            EventPeerClosing = null;

            SessionMgr.Destroy();
            _connectEvent.Dispose();
            _underlineSocket.Close();

            if (EventPeerClosing != null)
                EventPeerClosing();

            Logger.Ins.Debug("Client stopped!");
        }

        public void PerformInLogic(Action action)
        {
            LogicService.Perform(action);
        }

        public void PerformInLogic<TParam>(Action<TParam> action, TParam param)
        {
            LogicService.Perform(action, param);
        }

        public void PerformInNet(Action action)
        {
            NetService.Perform(action);
        }

        public void PerformInNet<TParam>(Action<TParam> action, TParam param)
        {
            NetService.Perform(action, param);
        }
        
        public void Reconnect()
        {
            _underlineSocket = SocketExt.CreateTcpSocket();
            Connect();
        }

        public void Connect()
        {
            _connectEvent.RemoteEndPoint = EndPoint;
            try
            {
                if (!_underlineSocket.ConnectAsync(_connectEvent))
                    HandleConnection(_underlineSocket);
            }
            catch (Exception e)
            {
                PerformInLogic(() =>
                {
                    var msg = string.Format("Connection failed, detail {0} : {1}", e.Message, e.StackTrace);
                    OnError(msg);

                    if (EventErrorCatched != null)
                        EventErrorCatched(msg);
                });
            }
        }

        private void HandleConnection(Socket sock)
        {
            var session = New<TSession>(new SessionArg(this, Uid.New(), sock), true);
            SessionMgr.AddSession(session);
        }
        
        private void OnConnectCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
                HandleConnection(_underlineSocket);
            else
            {
                PerformInLogic(() =>
                {
                    var msg = string.Format("Connection failed, detail {0}", e.SocketError);
                    OnError(msg);

                    if (EventErrorCatched != null)
                        EventErrorCatched(msg);
                });
            }
        }
    }
}
