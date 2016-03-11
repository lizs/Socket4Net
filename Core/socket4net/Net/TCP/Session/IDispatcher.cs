namespace socket4net
{
    /// <summary>
    ///     dispatch protocol
    /// </summary>
    public interface IDispatcher
    {
        IDataProtocol Unpack(byte[] data);
    }

    public interface IDataProtocol
    {
    }
}