#region MIT
//  /*The MIT License (MIT)
// 
//  Copyright 2016 lizs lizs4ever@163.com
//  
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//  
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
//  
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
//   * */
#endregion
#if NET45
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
