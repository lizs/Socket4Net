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
        ///     Push message to clients specified by 'sessions'
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="proto"></param>
        /// <param name="sessions"></param>
        void MultiCast<T>(T proto, IEnumerable<ISession> sessions) where T : IDataProtocol;
        
        /// <summary>
        ///     Push message to all the clients
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="proto"></param>
        void Broadcast<T>(T proto) where T : IDataProtocol;

#if NET45
        Task<NetResult> HandleRequest(IDataProtocol rq);
        Task<bool> HandlePush(IDataProtocol ps);
        Task<NetResult> RequestAsync<T>(T proto) where T : IDataProtocol;
#else
        /// <summary>
        /// </summary>
        /// <param name="rq"></param>
        /// <param name="cb"></param>
        void HandleRequest(IDataProtocol rq, Action<NetResult> cb);

        /// <summary>
        /// </summary>
        /// <param name="ps"></param>
        /// <param name="cb"></param>
        void HandlePush(IDataProtocol ps, Action<bool> cb);
#endif

        /// <summary>
        /// </summary>
        /// <param name="proto"></param>
        /// <param name="cb"></param>
        /// <typeparam name="T"></typeparam>
        void RequestAsync<T>(T proto, Action<bool, byte[]> cb) where T : IDataProtocol;

        /// <summary>
        /// </summary>
        /// <param name="proto"></param>
        /// <typeparam name="T"></typeparam>
        void Push<T>(T proto) where T : IDataProtocol;
    }
}