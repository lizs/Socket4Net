using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace socket4net
{
    public class ServerArg : PeerArg
    {
        public ServerArg(IObj parent, string ip, ushort port)
            : base(parent, ip, port)
        {
        }
    }

    public class Server<TSession> : Obj, IPeer
        where TSession : class, ISession, new()
    {
        private const int DefaultBacktrace = 10;
        public ushort Port { get; private set; }
        public string Ip { get; private set; }
        public IPAddress Address { get; private set; }
        public EndPoint EndPoint { get; private set; }

        public override string Name
        {
            get { return string.Format("{0}:{1}:{2}", typeof (TSession).Name, Ip, Port); }
        }

        public ILogicService LogicService
        {
            get { return Launcher.Instance.LogicService; }
        }

        public INetService NetService
        {
            get { return Launcher.Instance.NetService; }
        }

        public SessionMgr SessionMgr { get; private set; }

        public event Action<ISession, SessionCloseReason> EventSessionClosed;
        public event Action<ISession> EventSessionEstablished;
        public event Action<string> EventErrorCatched;
        public event Action EventPeerClosing;

        private Socket _listener;
        private SessionFactory<TSession> _sessionFactory;


        private SocketAsyncEventArgs _acceptEvent;

        private ConcurrentQueue<Socket> _clients;
        private AutoResetEvent _socketAcceptedEvent;
        private Thread _sessionFactoryWorker;
        private bool _quit;

        protected override void OnInit(ObjArg arg)
        {
            base.OnInit(arg);

            var more = arg.As<PeerArg>();

            Ip = more.Ip;
            Port = more.Port;

            IPAddress address;
            if (!IPAddress.TryParse(Ip, out address)) 
                Logger.Instance.FatalFormat("Invalid ip {0}", Ip);

            Address = address;
            EndPoint = new IPEndPoint(Address, Port);

            _sessionFactory = new SessionFactory<TSession>();

            SessionMgr = new SessionMgr(this,
                session =>
                {
                    if (EventSessionEstablished == null) return;

                    EventSessionEstablished(session as TSession);
                    OnConnected(session);
                },
                (session, reason) =>
                {
                    if (EventSessionClosed != null)
                        EventSessionClosed(session as TSession, reason);
                    OnDisconnected(session, reason);
                });
        }

        protected virtual void OnConnected(ISession session)
        {
            Logger.Instance.InfoFormat("{0} : {1} connected!", Name, session.Name);
        }

        protected virtual void OnDisconnected(ISession session, SessionCloseReason reason)
        {
            Logger.Instance.InfoFormat("{0} : {1} disconnected by {2}", Name, session.Name, reason);
        }

        protected virtual void OnError(string msg)
        {
            Logger.Instance.ErrorFormat("{0} : {1}", Name, msg);
        }

        protected override void OnStart()
        {
            base.OnStart();
            try
            {
                _listener = SocketExt.CreateTcpSocket();
                _listener.Bind(EndPoint);
                _listener.Listen(DefaultBacktrace);
            }
            catch (Exception e)
            {
                OnError(string.Format("Server start failed, detail {0} : {1}", e.Message, e.StackTrace));
                return;
            }

            _clients = new ConcurrentQueue<Socket>();

            _socketAcceptedEvent = new AutoResetEvent(false);
            _sessionFactoryWorker = new Thread(ProduceSessions) {Name = "SessionFactory"};
            _sessionFactoryWorker.Start();

            _acceptEvent = new SocketAsyncEventArgs();
            _acceptEvent.Completed += OnAcceptCompleted;

            AcceptNext();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventPeerClosing = null;
            EventSessionClosed = null;
            EventSessionEstablished = null;

            if (EventPeerClosing != null)
                EventPeerClosing();

            SessionMgr.Destroy();

            _listener.Close();
            if (_acceptEvent != null)
                _acceptEvent.Dispose();

            _quit = true;
            _socketAcceptedEvent.Set();
#if NET45
            _socketAcceptedEvent.Dispose();
#endif
            if (_sessionFactoryWorker != null)
                _sessionFactoryWorker.Join();
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

        private void ProduceSessions()
        {
            while (!_quit)
            {
                if (!_socketAcceptedEvent.WaitOne()) continue;

                Socket client;
                while (_clients.TryDequeue(out client))
                {
                    var session = _sessionFactory.Create(client, this);
                    session.Start();
                    SessionMgr.Add(session);
                }
            }
        }

        private void OnAcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                _clients.Enqueue(e.AcceptSocket);
                _socketAcceptedEvent.Set();

                AcceptNext();
            }
        }

        private void AcceptNext()
        {
            _acceptEvent.AcceptSocket = null;

            var result = true;
            try
            {
                // _listener在服务器stop时会抛出异常
                result = _listener.AcceptAsync(_acceptEvent);
            }
            catch (Exception e)
            {
                var msg = string.Format("Accept failed, detail {0} : {1}", e.Message, e.StackTrace);
                OnError(msg);

                if (EventErrorCatched != null)
                    EventErrorCatched(msg);
            }
            finally
            {
                if (!result)
                    HandleListenerError();
            }
        }

        private void HandleListenerError()
        {
            _listener.Close();
        }
    }
}
