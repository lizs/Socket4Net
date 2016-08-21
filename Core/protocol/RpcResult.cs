namespace socket4net
{
    /// <summary>
    ///     
    /// </summary>
    public class RpcResult : Pair<bool, byte[]>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item1"></param>
        /// <param name="item2"></param>
        public RpcResult(bool item1, byte[] item2)
            : base(item1, item2)
        {
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString() => Key.ToString();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rt"></param>
        /// <returns></returns>
        public static implicit operator bool(RpcResult rt)
        {
            return rt.Key;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ret"></param>
        /// <returns></returns>
        public static implicit operator RpcResult(bool ret)
        {
            return ret ? Success : Failure;
        }

        /// <summary>
        /// 
        /// </summary>
        public static RpcResult Success => new RpcResult(true, null);

        /// <summary>
        /// 
        /// </summary>
        public static RpcResult Failure => new RpcResult(false, null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static RpcResult MakeSuccess(byte[] data)
        {
            return new RpcResult(true, data);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="proto"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static RpcResult MakeSuccess<T>(T proto)
        {
            return new RpcResult(true, PiSerializer.Serialize(proto));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="proto"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static RpcResult MakeFailure<T>(T proto)
        {
            return new RpcResult(false, PiSerializer.Serialize(proto));
        }
    }
}