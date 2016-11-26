using ProtoBuf;
using socket4net;

namespace Pi
{
    /// <summary>
    ///     client2connector data protocol
    /// </summary>
    [ProtoContract]
    public class Client2ConnectorDataProtocol : IDataProtocol
    {
        /// <summary>
        ///     target backend server id
        /// </summary>
        [ProtoMember(1)]
        public short BackendId { get; set; }

        /// <summary>
        ///     operation code
        /// </summary>
        [ProtoMember(2)]
        public short Ops { get; set; }

        /// <summary>
        ///     operation data
        /// </summary>
        [ProtoMember(3)]
        public byte[] Data { get; set; }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString() => $"[{Ops}:{BackendId}:{(Data.IsNullOrEmpty() ? 0 : Data.Length)}bytes]";
    }

    /// <summary>
    ///     bind uid to session
    /// </summary>
    [ProtoContract]
    public class BindProto
    {
        /// <summary>
        ///     uid
        /// </summary>
        [ProtoMember(1)]
        public string Uid { get; set; }
    }
}