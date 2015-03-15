using System;
using System.Net.Sockets;

namespace Core.Net.TCP
{
    public interface ISessionFactory<out TSession> where TSession : ISession
    {
        TSession Create(Socket sock, IPeer hostPeer);
    }

    public class SessionFactory<TSession> : ISessionFactory<TSession> where TSession : ISession, new()
    {
        public TSession Create(Socket sock, IPeer hostPeer)
        {
            return new TSession { Id = GenUid(), UnderlineSocket = sock, HostPeer = hostPeer };
        }

        private long GenUid()
        {
            var buffer = Guid.NewGuid().ToByteArray();
            return BitConverter.ToInt64(buffer, 0);
        }
    }
}
