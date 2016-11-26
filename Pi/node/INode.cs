using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using socket4net;

namespace Pi.node
{
    /// <summary>
    ///     interface of node
    /// </summary>
    public interface INode : IUniqueObj<Guid>
    {
        /// <summary>
        ///     request(awaitable)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sid"></param>
        /// <param name="proto"></param>
        /// <returns></returns>
        Task<RpcResult> RequestAsync<T>(Guid sid, T proto) where T : IDataProtocol;

        /// <summary>
        ///     request(callback)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sid"></param>
        /// <param name="proto"></param>
        /// <param name="cb"></param>
        void RequestAsync<T>(Guid sid, T proto, Action<bool, byte[]> cb) where T : IDataProtocol;

        /// <summary>
        ///     push
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sid"></param>
        /// <param name="proto"></param>
        /// <returns></returns>
        bool Push<T>(Guid sid, T proto) where T : IDataProtocol;
    }

    /// <summary>
    ///     interface of server node
    /// </summary>
    public interface IServerNode : INode
    {
        /// <summary>
        ///     request(awaitable)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sid"></param>
        /// <param name="proto"></param>
        /// <returns></returns>
        Task<RpcResult> BroadcastAsync<T>(Guid sid, T proto) where T : IDataProtocol;

        /// <summary>
        ///     request(callback)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sid"></param>
        /// <param name="proto"></param>
        /// <param name="cb"></param>
        void BroadcastAsync<T>(Guid sid, T proto, Action<bool, byte[]> cb) where T : IDataProtocol;
    }
}