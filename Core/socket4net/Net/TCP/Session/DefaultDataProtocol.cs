using ProtoBuf;

namespace socket4net
{
    /// <summary>
    /// socket4net's default data protocol
    /// </summary>
    [ProtoContract]
    public class DefaultDataProtocol : IDataProtocol
    {
        [ProtoMember(1)]
        public short Ops { get; set; }
        [ProtoMember(2)]
        public byte[] Data { get; set; }

        public override string ToString()
        {
            return string.Format("[{0}:{1}bytes]", Ops, Data.IsNullOrEmpty() ? 0 : Data.Length);
        }
    }
}