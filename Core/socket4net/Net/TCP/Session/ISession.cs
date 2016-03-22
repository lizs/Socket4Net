using System.Net.Sockets;
#if NET45
using System.Threading.Tasks;
#endif

namespace socket4net
{
    /// <summary>
    ///     Session closed reason
    /// </summary>
    public enum SessionCloseReason
    {
        /// <summary>
        ///     By me
        /// </summary>
        ClosedByMyself,
        /// <summary>
        ///     By peer
        /// </summary>
        ClosedByRemotePeer,
        /// <summary>
        ///     Read error
        /// </summary>
        ReadError,
        /// <summary>
        ///     Write error
        /// </summary>
        WriteError,

        /// <summary>
        ///     Pack error
        ///     That's mean data can't packaged by DataParser or data is much too huge
        /// </summary>
        PackError,

        /// <summary>
        ///     Replaced
        /// </summary>
        Replaced,
    }

    /// <summary>
    ///     Session
    ///     Indicate a link between client and server
    /// </summary>
    public interface ISession : IUniqueObj<long>
    {
        /// <summary>
        ///     Socket attached to this session
        /// </summary>
        Socket UnderlineSocket { get;}

        /// <summary>
        /// 
        /// </summary>
        ushort ReceiveBufSize { get; }

        /// <summary>
        /// 
        /// </summary>
        ushort PackageMaxSize { get; }

        /// <summary>
        ///     Close this session
        /// </summary>
        /// <param name="reason"></param>
        void Close(SessionCloseReason reason);

        /// <summary>
        ///     Send message to peer
        /// </summary>
        /// <param name="pack"></param>
        void InternalSend(NetPackage pack);

#if NET35
        /// <summary>
        ///     Dispatch data
        /// </summary>
        /// <param name="data"></param>
        void Dispatch(byte[] data);
#else
        Task Dispatch(byte[] data);
#endif
    }
}