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
            get { return string.Format("{0}:{1}", Ip, Port); }
        }

        public ILogicService LogicService
        {
            get { return Launcher.Ins.LogicService; }
        }

        public INetService NetService
        {
            get { return Launcher.Ins.NetService; }
        }

        public SessionMgr SessionMgr { get; private set; }

        public event Action<ISession, SessionCloseReason> EventSessionClosed;
        public event Action<ISession> EventSessionEstablished;
        public event Action<string> EventErrorCatched;
        public event Action EventPeerClosing;

        private Socket _listener;
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
                Logger.Ins.Fatal("Invalid ip {0}", Ip);

            Address = address;
            EndPoint = new IPEndPoint(Address, Port);
            
            SessionMgr = New<SessionMgr>(new SessionMgrArg(this, session =>
                {
                    if (EventSessionEstablished != null)
                        EventSessionEstablished(session as TSession);
                    OnConnected(session);
                },
                (session, reason) =>
                {
                    if (EventSessionClosed != null)
                        EventSessionClosed(session as TSession, reason);
                    OnDisconnected(session, reason);
                }));
        }

        protected virtual void OnConnected(ISession session)
        {
            Logger.Ins.Info("{0}:{1} connected!", Name, session.Name);
        }

        protected virtual void OnDisconnected(ISession session, SessionCloseReason reason)
        {
            Logger.Ins.Info("{0}:{1} disconnected by {2}", Name, session.Name, reason);
        }

        protected virtual void OnError(string msg)
        {
            Logger.Ins.Error("{0}:{1}", Name, msg);
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
            Logger.Ins.Debug("Server started on {0}:{1}", Ip, Port);
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

            Logger.Ins.Info("Server stopped!");
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
            SocketAsyncEventArgs e;
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
                var msg = string.Format("Accept failed, detail {0} : {1}", e.Message, e.StackTrace);
                OnError(msg);

                if (EventErrorCatched != null)
                    EventErrorCatched(msg);

                AcceptNext();
            }
        }
    }
}
