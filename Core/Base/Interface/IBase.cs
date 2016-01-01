﻿#if NET45
using System.Threading.Tasks;
#endif

namespace socket4net
{   
    ///// <summary>
    /////     批量推送、存储接口
    ///// </summary>
    //public interface IBatched : IObj
    //{
    //    Counter BatchCounter { get; }
    //}

    /// <summary>
    /// 序列化接口
    /// byte[] 特化
    /// </summary>
    public interface ISerializable : ISerializable<byte[]>
    {
    }

    /// <summary>
    ///     泛型序列化接口
    /// </summary>
    public interface ISerializable<T>
    {
        T Serialize();
        bool Deserialize(T data);
    }

    /// <summary>
    ///     差分序列化接口
    /// </summary>
    public interface IDifferentialSerializable : IDifferentialSerializable<byte[]>
    {
    }

    /// <summary>
    ///     泛型差分序列化接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDifferentialSerializable<T> : ISerializable<T>
    {
        T SerializeDifference();
        bool DeserializeDifference(T data);
    }
    
    /// <summary>
    /// 远程调度接口
    /// </summary>
    public interface IRpc
    {
        IRpcSession Session { get; }

#if NET35
        /// <summary>
        /// 处理rpc请求
        /// </summary>
        /// <bytes name="ops"></bytes>
        /// <bytes name="bytes"></bytes>
        /// <returns></returns>
        RpcResult HandleRequest(short ops, byte[] bytes);
        /// <summary>
        /// 处理rpc推送
        /// </summary>
        /// <bytes name="ops"></bytes>
        /// <bytes name="bytes"></bytes>
        /// <returns></returns>
        bool HandlePush(short ops, byte[] bytes);
#else
        /// <summary>
        ///     处理rpc请求
        /// </summary>
        /// <bytes name="ops"></bytes>
        /// <bytes name="bytes"></bytes>
        /// <returns></returns>
        Task<RpcResult> HandleRequest(short ops, byte[] bytes);

        /// <summary>
        ///     处理rpc推送
        /// </summary>
        /// <bytes name="ops"></bytes>
        /// <bytes name="data"></bytes>
        /// <returns></returns>
        Task<bool> HandlePush(short ops, byte[] data);
#endif
    }

    /// <summary>
    /// 从字符串解析
    /// </summary>
    public interface IParseFromString
    {
        void ParseFromString(string str);
    }

    /// <summary>
    ///    克隆
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICloneable<in T>
    {
        void CloneFrom(T other);
    }
}
