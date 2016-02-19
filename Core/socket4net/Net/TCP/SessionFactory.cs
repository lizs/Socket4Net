using System;
using System.Net.Sockets;

namespace socket4net
{
    //public interface ISessionFactory<out TSession> where TSession : ISession
    //{
    //    TSession Create(Socket sock, IPeer hostPeer);
    //}

    public class SessionFactory<TSession> where TSession : ISession, new()
    {
        public TSession Create(Socket sock, IPeer hostPeer)
        {
            return Obj.Create<TSession>(new SessionArg(hostPeer, GetGUID(), sock), true);
        }

        private long GetGUID()
        {
            var buffer = Guid.NewGuid().ToByteArray();
            return BitConverter.ToInt64(buffer, 0);
        }
    }
}
