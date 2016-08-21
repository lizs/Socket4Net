using ProtoBuf;

namespace socket4net
{
    /// <summary>
    ///     data transform protocol
    /// </summary>
    public interface IDataProtocol
    {
    }

    /// <summary>
    /// socket4net's default data protocol
    /// </summary>
    [ProtoContract]
    public class DefaultDataProtocol : IDataProtocol
    {
        /// <summary>
        ///     operation id
        /// </summary>
        [ProtoMember(1)]
        public short Ops { get; set; }

        /// <summary>
        ///     operation data
        /// </summary>
        [ProtoMember(2)]
        public byte[] Data { get; set; }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString() => $"[{Ops}:{(Data.IsNullOrEmpty() ? 0 : Data.Length)}bytes]";
    }
}