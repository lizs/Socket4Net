using System;
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
        public ClientArg(IObj parent, string ip, ushort port)
            : base(parent, ip, port, GlobalVarPool.Instance.LogicService, GlobalVarPool.Instance.NetService)
        {
        }

        public bool AutoReconnectEnabled { get; set; }
    }

    public class Client<TSession, TLogicService, TNetService> : ScheduledObj, IClient
        where TSession : class, ISession, new()
        where TNetService : class ,INetService, new()
        where TLogicService : class, ILogicService, new()
    {
        public const uint ReconnectDefaultDelay = 1000;
        public const uint ReconnectMaxDelay = 32 * 1000;
        public event Action<ISession, SessionCloseReason> EventSessionClosed;
        public event Action<ISession> EventSessionEstablished;
        public event Action EventPeerClosing;
        public event Action<string> EventErrorCatched;

        public override string Name
        {
            get { return string.Format("{0}:{1}:{2}", typeof(TSession).Name, Ip, Port); }
        }

        public string Ip { get; private set; }
        public ushort Port { get; private set; }
        public IPAddress Address { get; private set; }
        public EndPoint EndPoint { get; private set; }
        public SessionMgr SessionMgr { get; private set; }
        public bool IsLogicServiceShared { get; private set; }
        public bool IsNetServiceShared { get; private set; }
        public ILogicService LogicService { get; private set; }
        public INetService NetService { get; private set; }


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
        private SessionFactory<TSession> _sessionFactory;
        private SocketAsyncEventArgs _connectEvent;
        private bool _connected;
        private uint _reconnectRetryDelay = ReconnectDefaultDelay;

        protected override void OnInit(ObjArg arg)
        {
            base.OnInit(arg);

            var more = arg as ClientArg;
            LogicService = more.LogicService;
            NetService = more.NetService;
            AutoReconnectEnabled = more.AutoReconnectEnabled;

            if(string.IsNullOrEmpty(more.Ip) || more.Port == 0)
                Logger.Instance.Warn("Ip or Port is invalid!");
            else if (!SetAddress(more.Ip, more.Port))
                throw new Exception("Ip or Port is invalid!");

            _sessionFactory = new SessionFactory<TSession>();
            SessionMgr = new SessionMgr(this,
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
                });
        }

        protected virtual void OnConnected(ISession session)
        {
            Connected = true;
            Logger.Instance.InfoFormat("{0} : {1} connected!", Name, session.Name);
        }

        protected virtual void OnDisconnected(ISession session, SessionCloseReason reason)
        {
            Connected = false;
            Logger.Instance.InfoFormat("{0} : {1} disconnected by {2}", Name, session.Name, reason);

            if (AutoReconnectEnabled)
            {
                Invoke(Reconnect, ReconnectRetryDelay);
            }
        }

        protected virtual void OnError(string msg)
        {
            Connected = false;
            Logger.Instance.ErrorFormat("{0} : {1}", Name, msg);

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

            IsLogicServiceShared = (LogicService != null);
            IsNetServiceShared = (NetService != null);

            LogicService = (LogicService as TLogicService) ?? new TLogicService { Capacity = 10000, Period = 10 };
            if (!IsLogicServiceShared) LogicService.Start();

            NetService = (NetService as TNetService) ?? new TNetService { Capacity = 10000, Period = 10 };
            if (!IsNetServiceShared) NetService.Start();

            GlobalVarPool.Instance.Set(GlobalVarPool.NameOfLogicService, LogicService);
            GlobalVarPool.Instance.Set(GlobalVarPool.NameOfNetService, NetService);

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

            if (!IsLogicServiceShared) LogicService.Stop();
            if (!IsNetServiceShared) NetService.Stop();

            Logger.Instance.Debug("Client stopped!");
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

        public void Send<T>(T proto) where T : IProtobufInstance
        {
            if (Session != null) Session.Send(proto);
        }

        public void Send(byte[] data)
        {
            if (Session != null) Session.Send(data);
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
            var session = _sessionFactory.Create(sock, this);
            SessionMgr.Add(session);
            Session.Start();
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
