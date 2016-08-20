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

namespace socket4net
{
    /// <summary>
    /// 
    /// </summary>
    public interface IClient : IPeer
    {
        /// <summary>
        ///     Reconnect to server
        /// </summary>
        void Reconnect();

        /// <summary>
        ///     Connect to server
        /// </summary>
        void Connect();

        /// <summary>
        ///     If client is connected to server
        /// </summary>
        bool Connected { get; }
    }
    
    /// <summary>
    /// 
    /// </summary>
    public class ClientArg : PeerArg
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="autoReconnectEnabled"></param>
        public ClientArg(IObj parent, string ip, ushort port, bool autoReconnectEnabled = false)
            : base(parent, ip, port)
        {
            AutoReconnectEnabled = autoReconnectEnabled;
        }

        /// <summary>
        ///  wether auto reconnect enabled
        /// </summary>
        public bool AutoReconnectEnabled { get; private set; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TSession"></typeparam>
    public class Client<TSession> : Obj, IClient
        where TSession : class, ISession, new()
    {
        /// <summary>
        ///     Default reconnect retry delay time
        /// </summary>
        public const uint ReconnectDelay = 1000;
        /// <summary>
        ///     Default reconnect retry max delay time
        /// </summary>
        public const uint ReconnectMaxDelay = 32 * 1000;

        /// <summary>
        ///     Raised when a session closed
        /// </summary>
        public event Action<ISession, SessionCloseReason> EventSessionClosed;

        /// <summary>
        ///     Raised when a session established
        /// </summary>
        public event Action<ISession> EventSessionEstablished;

        /// <summary>
        ///     Raised when Peer closing
        /// </summary>
        public event Action EventPeerClosing;

        /// <summary>
        ///     Raised when error catched
        /// </summary>
        public event Action<string> EventErrorCatched;

        /// <summary>
        ///     name
        /// </summary>
        public override string Name => $"{Ip}:{Port}";

        /// <summary>
        /// 
        /// </summary>
        public string Ip { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public ushort Port { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public IPAddress Address { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public EndPoint EndPoint { get; private set; }

        /// <summary>
        ///     Get session manager
        /// </summary>
        public SessionMgr SessionMgr { get; private set; }

        /// <summary>
        ///     Get logic service
        /// </summary>
        public ILogicService LogicService => Launcher.Ins.LogicService;

        /// <summary>
        ///     Get net service
        /// </summary>
        public ITcpService TcpService => Launcher.Ins.TcpService;

        /// <summary>
        ///     If client is connected to server
        /// </summary>
        public bool Connected
        {
            get { return _connected; }
            private set
            {
                _connected = value;
                ReconnectRetryDelay = _connected
                    ? ReconnectDelay
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
        private uint ReconnectRetryDelay { get; set; } = ReconnectDelay;

        /// <summary>
        ///     get the underline session
        /// </summary>
        public TSession Session
        {
            get
            {
                var ret = SessionMgr.FirstOrDefault();
                return ret as TSession;
            }
        }

        private Socket _underlineSocket;
        private SocketAsyncEventArgs _connectEvent;
        private bool _connected;

        /// <summary>
        ///    internal called when an Obj is initialized
        /// </summary>
        /// <param name="arg"></param>
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
                    EventSessionEstablished?.Invoke(session as TSession);
                },
                (session, reason) =>
                {
                    OnDisconnected(session, reason);
                    EventSessionClosed?.Invoke(session as TSession, reason);
                }));
        }

        /// <summary>
        ///     invoked when session established
        /// </summary>
        /// <param name="session"></param>
        protected virtual void OnConnected(ISession session)
        {
            Connected = true;
            Logger.Ins.Info("{0}:{1} connected!", Name, session.Name);
        }

        /// <summary>
        /// invoked when session closed
        /// </summary>
        /// <param name="session"></param>
        /// <param name="reason"></param>
        protected virtual void OnDisconnected(ISession session, SessionCloseReason reason)
        {
            Connected = false;
            Logger.Ins.Info("{0}:{1} disconnected by {2}", Name, session.Name, reason);

            if (AutoReconnectEnabled)
            {
                (this as IObj).Invoke(Reconnect, ReconnectRetryDelay);
            }
        }

        /// <summary>
        /// invoked when session error occured
        /// </summary>
        /// <param name="msg"></param>
        protected virtual void OnError(string msg)
        {
            Connected = false;
            Logger.Ins.Error("{0}:{1}", Name, msg);

            if (AutoReconnectEnabled)
            {
                (this as IObj).Invoke(Reconnect, ReconnectRetryDelay);
            }
        }

        /// <summary>
        /// set the target server ip/port 
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <returns></returns>
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
                var msg = $"{e.Message} : {e.StackTrace}";
                OnError(msg);
                EventErrorCatched?.Invoke(msg);

                Ip = string.Empty;
                Port = 0;
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Invoked when obj started
        /// </summary>
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

        /// <summary>
        ///    internal called when an Obj is to be destroyed
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventSessionClosed = null;
            EventSessionEstablished = null;
            EventPeerClosing = null;

            SessionMgr.Destroy();
            _connectEvent.Dispose();
            _underlineSocket.Close();

            EventPeerClosing?.Invoke();
            Logger.Ins.Debug("Client stopped!");
        }

        /// <summary>
        ///     Excute 'action' in logic service
        /// </summary>
        /// <param name="action"></param>
        public void PerformInLogic(Action action)
        {
            LogicService.Perform(action);
        }

        /// <summary>
        ///     Excute 'action' in logic service
        /// </summary>
        /// <param name="action"></param>
        /// <param name="param"></param>
        public void PerformInLogic<TParam>(Action<TParam> action, TParam param)
        {
            LogicService.Perform(action, param);
        }

        /// <summary>
        ///     Excute 'action' in net service
        /// </summary>
        /// <param name="action"></param>
        public void PerformInNet(Action action)
        {
            TcpService.Perform(action);
        }

        /// <summary>
        ///     Excute 'action' in net service
        /// </summary>
        /// <param name="action"></param>
        /// <param name="param"></param>
        public void PerformInNet<TParam>(Action<TParam> action, TParam param)
        {
            TcpService.Perform(action, param);
        }

        /// <summary>
        ///     Reconnect to server
        /// </summary>
        public void Reconnect()
        {
            _underlineSocket = SocketExt.CreateTcpSocket();
            Connect();
        }

        /// <summary>
        ///     Connect to server
        /// </summary>
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
                    var msg = $"Connection failed, detail {e.Message} : {e.StackTrace}";
                    OnError(msg);
                    EventErrorCatched?.Invoke(msg);
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
                    var msg = $"Connection failed, detail {e.SocketError}";
                    OnError(msg);
                    EventErrorCatched?.Invoke(msg);
                });
            }
        }
    }
}
