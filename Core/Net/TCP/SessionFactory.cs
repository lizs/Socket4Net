using System;
using System.Net.Sockets;

namespace Core.Net.TCP
{
    public interface ISessionFactory
    {
        ISession Create(Socket sock);
    }

    public class SessionFactory<T> : ISessionFactory where T : Session, new()
    {
        public ISession Create(Socket sock)
        {
            return new T { Id = GenUid(), UnderlineSocket = sock, Packer = new Packer() };
        }

        private long GenUid()
        {
            var buffer = Guid.NewGuid().ToByteArray();
            return BitConverter.ToInt64(buffer, 0);
        }
    }
}
