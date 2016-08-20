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
using System.Net;
using System.Net.Sockets;
using System.Threading;
#if NET45
using System.Collections.Concurrent;
#endif

namespace socket4net
{
    /// <summary>
    ///     
    /// </summary>
    public class ServerArg : PeerArg
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public ServerArg(IObj parent, string ip, ushort port)
            : base(parent, ip, port)
        {
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TSession"></typeparam>
    public class Server<TSession> : Obj, IPeer
        where TSession : class, ISession, new()
    {
        private const int DefaultBacktrace = 10;

        /// <summary>
        /// 
        /// </summary>
        public ushort Port { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public string Ip { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public IPAddress Address { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public EndPoint EndPoint { get; private set; }

        /// <summary>
        ///     name
        /// </summary>
        public override string Name => $"{Ip}:{Port}";

        /// <summary>
        ///     Get logic service
        /// </summary>
        public ILogicService LogicService => Launcher.Ins.LogicService;

        /// <summary>
        ///     Get net service
        /// </summary>
        public ITcpService TcpService => Launcher.Ins.TcpService;

        /// <summary>
        ///     回话管理器
        ///     线程安全
        /// </summary>
        public SessionMgr SessionMgr { get; private set; }

        /// <summary>
        ///     Raised when a session closed
        /// </summary>
        public event Action<ISession, SessionCloseReason> EventSessionClosed;

        /// <summary>
        ///     Raised when a session established
        /// </summary>
        public event Action<ISession> EventSessionEstablished;

        /// <summary>
        ///     Raised when error catched
        /// </summary>
        public event Action<string> EventErrorCatched;

        /// <summary>
        ///     Raised when Peer closing
        /// </summary>
        public event Action EventPeerClosing;

        private Socket _listener;
        private SocketAsyncEventArgs _acceptEvent;

        private ConcurrentQueue<Socket> _clients;
        private AutoResetEvent _socketAcceptedEvent;
        private Thread _sessionFactoryWorker;
        private bool _quit;

        /// <summary>
        ///    internal called when an Obj is initialized
        /// </summary>
        /// <param name="arg"></param>
        protected override void OnInit(ObjArg arg)
        {
            base.OnInit(arg);

            var more = arg.As<PeerArg>();

            Ip = more.Ip;
            Port = more.Port;

            IPAddress address;
            if (!IPAddress.TryParse(Ip, out address)) 
                Logger.Ins.Fatal("Invalid ip {0}", Ip);

            Address = address;
            EndPoint = new IPEndPoint(Address, Port);
            
            SessionMgr = New<SessionMgr>(new SessionMgrArg(this, session =>
                {
                    EventSessionEstablished?.Invoke(session as TSession);
                    OnConnected(session);
                },
                (session, reason) =>
                {
                    EventSessionClosed?.Invoke(session as TSession, reason);
                    OnDisconnected(session, reason);
                }));
        }

        /// <summary>
        ///     session established
        /// </summary>
        /// <param name="session"></param>
        protected virtual void OnConnected(ISession session)
        {
            Logger.Ins.Info("{0}:{1} connected!", Name, session.Name);
        }

        /// <summary>
        /// session closed
        /// </summary>
        /// <param name="session"></param>
        /// <param name="reason"></param>
        protected virtual void OnDisconnected(ISession session, SessionCloseReason reason)
        {
            Logger.Ins.Info("{0}:{1} disconnected by {2}", Name, session.Name, reason);
        }

        /// <summary>
        /// error occured
        /// </summary>
        /// <param name="msg"></param>
        protected virtual void OnError(string msg)
        {
            Logger.Ins.Error("{0}:{1}", Name, msg);
        }

        /// <summary>
        ///     Invoked when obj started
        /// </summary>
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
                OnError($"Server start failed, detail {e.Message} : {e.StackTrace}");
                return;
            }

            _clients = new ConcurrentQueue<Socket>();

            _socketAcceptedEvent = new AutoResetEvent(false);
            _sessionFactoryWorker = new Thread(ProduceSessions) {Name = "SessionFactory"};
            _sessionFactoryWorker.Start();

            _acceptEvent = new SocketAsyncEventArgs();
            _acceptEvent.Completed += OnAcceptCompleted;

            AcceptNext();
            Logger.Ins.Debug("Server started on {0}:{1}", Ip, Port);
        }

        /// <summary>
        ///    internal called when an Obj is to be destroyed
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();

            EventPeerClosing = null;
            EventSessionClosed = null;
            EventSessionEstablished = null;

            EventPeerClosing?.Invoke();
            SessionMgr.Destroy();

            _listener.Close();
            _acceptEvent?.Dispose();

            _quit = true;
            _socketAcceptedEvent.Set();
#if NET45
            _socketAcceptedEvent.Dispose();
#endif
            _sessionFactoryWorker?.Join();

            Logger.Ins.Info("Server stopped!");
        }

        /// <summary>
        ///     Excute 'action' in logic service
        /// </summary>
        /// <param name="action"></param>
        void IPeer.PerformInLogic(Action action)
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

        private void ProduceSessions()
        {
            while (!_quit)
            {
                if (!_socketAcceptedEvent.WaitOne()) continue;

                Socket client;
                while (_clients.TryDequeue(out client))
                {
                    var session = New<TSession>(new SessionArg(this, Uid.New(), client), true);
                    SessionMgr.AddSession(session);
                }
            }
        }

        private void OnAcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            ProcessAccept(e.AcceptSocket, e.SocketError);
        }

        private void ProcessAccept(Socket sock, SocketError error)
        {
            if (error != SocketError.Success)
            {
                Logger.Ins.Error("Listener down!");
            }
            else
            {
                _clients.Enqueue(sock);
                _socketAcceptedEvent.Set();
                AcceptNext();
            }
        }

        private void AcceptNext()
        {
            _acceptEvent.AcceptSocket = null;

            try
            {
                if (!_listener.AcceptAsync(_acceptEvent))
                {
                    ProcessAccept(_acceptEvent.AcceptSocket, _acceptEvent.SocketError);
                }
            }
            catch (Exception e)
            {
                var msg = $"Accept failed, detail {e.Message} : {e.StackTrace}";
                OnError(msg);

                EventErrorCatched?.Invoke(msg);

                AcceptNext();
            }
        }
    }
}
