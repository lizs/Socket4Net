namespace socket4net.RPC
{  
    /// <summary>
    /// Rpc类型
    /// C->S or S->C
    /// </summary>
    public enum RpcType
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
}
