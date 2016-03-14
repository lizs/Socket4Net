using System;
using System.Collections.Generic;
#if NET45
using System.Threading.Tasks;
#endif

namespace socket4net
{
    /// <summary>
    ///     diapatchable session
    ///     dispath request/push by DispatchProto
    /// </summary>
    public interface IDispatchableSession : ISession
    {
        /// <summary>
        /// ¶à²¥
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="proto"></param>
        /// <param name="sessions"></param>
        void MultiCast<T>(T proto, IEnumerable<ISession> sessions) where T : IDataProtocol;
        
        /// <summary>
        /// ¹ã²¥
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="proto"></param>
        void Broadcast<T>(T proto) where T : IDataProtocol;

#if NET45
        Task<NetResult> HandleRequest(IDataProtocol rq);
        Task<bool> HandlePush(IDataProtocol ps);
        Task<NetResult> RequestAsync<T>(T proto) where T : IDataProtocol;
#else
        void HandleRequest(IDataProtocol rq, Action<NetResult> cb);
        void HandlePush(IDataProtocol ps, Action<bool> cb);
#endif
        void RequestAsync<T>(T proto, Action<bool, byte[]> cb) where T : IDataProtocol;
        void Push<T>(T proto) where T : IDataProtocol;
    }
}