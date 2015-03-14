using System;
using System.Net;
using System.Net.Sockets;
using Core.Service;

namespace Core.Net.TCP
{
    public interface ITcpClient
    {
        event Action<ISession, ITcpClient> EventSessionEstablished;
        void Connect(byte[] info = null);
        void Shutdown();
        void Send(byte[] data);
        void Send<T>(T proto);
        ISession Session { get; }
    }

    public class TcpClient<T> : ITcpClient where T : Session, new()
    {
        private readonly Socket _underlineSocket;
        private readonly IPEndPoint _endpoint;
        private readonly SessionFactory<T> _sessionFactory;
        private readonly SocketAsyncEventArgs _connectEvent;

        public ISession Session { get; private set; }
        public event Action<ISession, ITcpClient> EventSessionEstablished;

        public TcpClient(string ip, ushort port)
        {
            _sessionFactory = new SessionFactory<T>();

            var addr = IPAddress.Parse(ip);
            _endpoint = new IPEndPoint(addr, port);

            _connectEvent = new SocketAsyncEventArgs();
            _connectEvent.Completed += OnConnectCompleted;

            _underlineSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Shutdown()
        {
            Session.Close(SessionCloseReason.ClosedByMyself);
            _connectEvent.Completed -= OnConnectCompleted;
        }

        public void Send<TP>(TP proto)
        {
            if (Session != null) Session.Send(proto);
        }

        public void Send(byte[] data)
        {
            if (Session != null) Session.Send(data);
        }

        public void Connect(byte[] info = null)
        {
            _connectEvent.RemoteEndPoint = _endpoint;
            if(info != null)
                _connectEvent.SetBuffer(info, 0, info.Length);
            if (!_underlineSocket.ConnectAsync(_connectEvent))
                HandleConnection(_connectEvent.ConnectSocket);
        }

        private void HandleConnection(Socket sock)
        {
            Session = _sessionFactory.Create(sock);
            Session.Start();

            StaService.Perform(() =>
            {
                if (EventSessionEstablished != null)
                    EventSessionEstablished(Session, this);
            });
        }
        
        private void OnConnectCompleted(object sender, SocketAsyncEventArgs e)
        {
            if(e.SocketError == SocketError.Success)
                HandleConnection(e.ConnectSocket);
            else
                NetLogger.Log.Error(e.SocketError);
        }
    }
}
