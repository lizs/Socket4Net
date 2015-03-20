using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Core.Log;
using Core.RPC;
using Core.Service;
using System.Collections.Concurrent;

namespace Core.Net.TCP
{
    public class Server<TSession, TNetService, TLogicService> : IPeer<TSession, TLogicService, TNetService>
        where TSession : class, ISession, new() 
        where TNetService : class ,INetService, new()
        where TLogicService : class ,ILogicService, new()
    {
        public ushort Port { get; private set; }
        public string Ip { get; private set; }
        public IPAddress Address { get; private set; }
        public EndPoint EndPoint { get; private set; }

        public bool IsLogicServiceShared { get; private set; }
        public bool IsNetServiceShared { get; private set; }
        public ILogicService LogicService { get; private set; }
        public INetService NetService { get; private set; }

        public SessionMgr SessionMgr { get; private set; }

        public event Action<TSession, SessionCloseReason> EventSessionClosed;
        public event Action<TSession, byte[]> EventSessionEstablished;
        public event Action EventPeerClosing;

        private Socket _listener;
        private readonly SessionFactory<TSession> _sessionFactory;

        private const int DefaultBacktrace = 10;
        private const int AcceptBufLen = 1024;
        private const int DefaultServiceCapacity = 10000;
        private const int DefaultServicePeriod = 10;

        private SocketAsyncEventArgs _acceptEvent;
        private CircularBuffer _acceptBuffer;

        private ConcurrentQueue<Tuple<Socket, byte[]>> _clients;
        private AutoResetEvent _socketAcceptedEvent;
        private Thread _sessionFactoryWorker;
        private bool _quit;

        public Server(string ip, ushort port)
        {
            Ip = ip;
            Port = port;

            IPAddress address;
            if(!IPAddress.TryParse(Ip, out address)) Logger.Instance.FatalFormat("Invalid ip [{0}]", ip);

            Address = address;
            EndPoint = new IPEndPoint(Address, Port);

            _sessionFactory = new SessionFactory<TSession>();

            SessionMgr = new SessionMgr(this,
                (session, data) => { if (EventSessionEstablished != null)EventSessionEstablished(session as TSession, data); },
                (session, reason) => { if (EventSessionClosed != null)EventSessionClosed(session as TSession, reason); });
        }

        /// <summary>
        /// 启动
        /// 由于同一进程可以启动若干server，这些server可以
        /// 拥有自己独立的服务，亦可以共享服务
        /// </summary>
        /// <param name="net">网络收发服务</param>
        /// <param name="logic">逻辑处理服务</param>
        public void Start(IService net, IService logic, byte[] data)
        {
            try
            {
                _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _listener.Bind(EndPoint);
                _listener.Listen(DefaultBacktrace);
            }
            catch (Exception e)
            {
                Logger.Instance.Fatal(e.Message);
                return;
            }

            _clients = new ConcurrentQueue<Tuple<Socket, byte[]>>();

            _socketAcceptedEvent = new AutoResetEvent(false);
            _sessionFactoryWorker = new Thread(ProduceSessions);
            _sessionFactoryWorker.Start();

            _acceptBuffer = new CircularBuffer(AcceptBufLen);
            _acceptEvent = new SocketAsyncEventArgs();
            _acceptEvent.Completed += OnAcceptCompleted;

            IsLogicServiceShared = (logic != null);
            IsNetServiceShared = (net != null);

            LogicService = (logic as TLogicService) ?? new TLogicService { Capacity = DefaultServiceCapacity, Period = DefaultServicePeriod };
            if (!IsLogicServiceShared) LogicService.Start();

            NetService = (net as TNetService) ?? new TNetService { Capacity = DefaultServiceCapacity, Period = DefaultServicePeriod };
            if (!IsNetServiceShared) NetService.Start();

            AcceptNext();
        }

        public void Stop()
        {
            LogicService.Perform(() =>
            {
                if (EventPeerClosing != null)
                    EventPeerClosing();

                SessionMgr.Dispose();

                _listener.Close();
                if(_acceptEvent != null ) _acceptEvent.Dispose();

                _quit = true;
                if(_sessionFactoryWorker != null ) _sessionFactoryWorker.Join();

                if (LogicService != null && !IsLogicServiceShared) LogicService.Stop();
                if (NetService != null && !IsNetServiceShared) NetService.Stop();
            });
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
                if (_socketAcceptedEvent.WaitOne())
                {
                    Tuple<Socket, byte[]> client;
                    while (_clients.TryDequeue(out client))
                    {
                        var session = _sessionFactory.Create(client.Item1, this);
                        session.Start();
                        SessionMgr.Add(session, client.Item2);
                    }
                }
            }
        }

        private void OnAcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
//                 byte[] data = new byte[e.BytesTransferred];
//                 Buffer.BlockCopy(e.Buffer, 0, data, 0, e.BytesTransferred);

                _clients.Enqueue(new Tuple<Socket, byte[]>(e.AcceptSocket, null));
                _socketAcceptedEvent.Set();
            }

            AcceptNext();
        }

        private void AcceptNext()
        {
            _acceptEvent.AcceptSocket = null;

            var result = true;
            try
            {
                // _listener在服务器stop时会抛出异常
                //_acceptEvent.SetBuffer(_acceptBuffer.Buffer, 0, AcceptBufLen);
                result = _listener.AcceptAsync(_acceptEvent);
            }
            catch (ObjectDisposedException e)
            {
                Logger.Instance.Error(e.Message);
            }
            catch (InvalidOperationException e)
            {
                Logger.Instance.Error(e.Message);
            }
            catch (Exception e)
            {
                Logger.Instance.Error(e.Message);
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

    public class Server<TSession> : Server<TSession, NetService, LogicService> where TSession : class, ISession, new()
    {
        public Server(string ip, ushort port) : base(ip, port)
        {
        }
    }
}
