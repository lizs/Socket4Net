using socket4net.Util;

namespace socket4net
{
    /// <summary>
    /// Rpc类型
    /// C->S or S->C
    /// </summary>
    public enum ERpc
    {
        /// <summary>
        /// 通知
        /// 无需等待对端响应
        /// </summary>
        Push,
        /// <summary>
        /// 请求
        /// 需要等待对端响应才能完成本次Rpc
        /// </summary>
        Request,
        /// <summary>
        /// 响应Request
        /// </summary>
        Response,
    }


    public class RpcResult : Pair<bool, byte[]>
    {
        public RpcResult(bool item1, byte[] item2)
            : base(item1, item2)
        {
        }

        public override string ToString()
        {
            return Key.ToString();
        }

        public static implicit operator bool(RpcResult rt)
        {
            return rt.Key;
        }

        public static implicit operator RpcResult(bool ret)
        {
            return ret ? Success : Failure;
        }

        public static RpcResult Success
        {
            get { return new RpcResult(true, null); }
        }

        public static RpcResult Failure
        {
            get { return new RpcResult(false, null); }
        }

        public static RpcResult MakeSuccess(byte[] data)
        {
            return new RpcResult(true, data);
        }

        public static RpcResult MakeSuccess<T>(T proto) where T : IProtobufInstance
        {
            return new RpcResult(true, PiSerializer.Serialize(proto));
        }

        public static RpcResult MakeFailure<T>(T proto) where T : IProtobufInstance
        {
            return new RpcResult(false, PiSerializer.Serialize(proto));
        }
    }

    public class RpcResult<T> : RpcResult where T : IProtobufInstance
    {
        public RpcResult(bool item1, byte[] item2) : base(item1, item2) { }
        public RpcResult(bool item1, T proto) : base(item1, PiSerializer.Serialize(proto)) { }
    }
}
