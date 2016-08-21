namespace socket4net
{
    /// <summary>
    ///     rpc type
    /// </summary>
    public enum ERpc
    {
        /// <summary>
        ///     notify
        ///     needn't an response
        /// </summary>
        Push,

        /// <summary>
        ///     request
        ///     response needed
        /// </summary>
        Request,

        /// <summary>
        ///     response
        ///     used internal
        /// </summary>
        Response
    }
}