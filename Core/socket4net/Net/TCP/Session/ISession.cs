using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace socket4net
{
    public enum SessionCloseReason
    {
        ClosedByMyself,
        ClosedByRemotePeer,
        ReadError,
        WriteError,
        PackError,

        /// <summary>
        /// 比如同一账号使用不同会话时，会顶掉之前会话
        /// </summary>
        Replaced,
    }

    public interface ISession : IUniqueObj<long>
    {
        Socket UnderlineSocket { get;}
        ushort ReceiveBufSize { get; }
        ushort PackageMaxSize { get; }

        void Close(SessionCloseReason reason);
        void InternalSend(NetPackage pack);

#if NET35
        void Dispatch(byte[] data);
#else
        Task Dispatch(byte[] data);
#endif
    }
}