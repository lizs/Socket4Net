namespace node
{
    public enum NodeId
    {
        /// <summary>
        /// 连接服
        /// </summary>
        C_Server4Auth,
        C_Server4Client,
        C_Server4Logic,

        /// <summary>
        /// 验证服
        /// </summary>
        A_Client2Connector,

        /// <summary>
        /// 数据服
        /// </summary>
        //P_Server4Logic,

        /// <summary>
        /// 逻辑服
        /// </summary>
        L_Server4Client,
        L_Client2Connector,
        L_Client2Persister,
    }
}