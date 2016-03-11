using ProtoBuf;

namespace socket4net
{
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

    public class DefaultDispatcher : IDispatcher
    {
        public IDataProtocol Unpack(byte[] data)
        {
            return PiSerializer.Deserialize<DefaultDataProtocol>(data);
        }
    }
}