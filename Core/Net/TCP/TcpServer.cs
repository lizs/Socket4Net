using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Core.Log;
using Core.Service;

#if !NET35
using System.Collections.Concurrent;
#else
using Core.ConcurrentCollection;
#endif

namespace Core.Net.TCP
{
    public interface ITcpServer
    {
        void Startup();
        void Shutdown();
    }

    public class TcpServer<T> : ITcpServer where T : Session, new()
    {
        private Socket _listener;
        private readonly ushort _port;
        private readonly string _ip;
        private readonly SessionFactory<T> _sessionFactory;

        private const int DefaultBacktrace = 10;
        private SocketAsyncEventArgs _acceptEvent;

        private ConcurrentQueue<Socket> _clients;
        private AutoResetEvent _socketAcceptedEvent;
        private Thread _sessionFactoryWorker;
        private bool _quit;
        
        public TcpServer(string ip, ushort port)
        {
            _ip = ip;
            _port = port;
            _sessionFactory = new SessionFactory<T>();
        }

        public void Startup()
        {
            var localAddr = IPAddress.Parse(_ip);
            var endpoint = new IPEndPoint(localAddr, _port);

            _clients = new ConcurrentQueue<Socket>();

            _socketAcceptedEvent = new AutoResetEvent(false);
            _sessionFactoryWorker = new Thread(ProduceSessions);
            _sessionFactoryWorker.Start();

            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listener.Bind(endpoint);
            _listener.Listen(DefaultBacktrace);

            _acceptEvent = new SocketAsyncEventArgs();
            _acceptEvent.Completed += OnAcceptCompleted;

            AcceptNext();
        }

        public void Shutdown()
        {
            _quit = true;
            _sessionFactoryWorker.Join();

            _listener.Close();
            _acceptEvent.Dispose();
            
            SessionMgr.Clear();
        }

        private void ProduceSessions()
        {
            while (!_quit)
            {
                if (_socketAcceptedEvent.WaitOne())
                {
                    Socket sock;
                    while (_clients.TryDequeue(out sock))
                    {
                        var session = _sessionFactory.Create(sock);
                        session.Start();
                        SessionMgr.Add(session);
                    }
                }
            }
        }

        private void OnAcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                _clients.Enqueue(e.AcceptSocket);
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
}
