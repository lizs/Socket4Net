using ProtoBuf;

namespace socket4net.BaseProto
{
    /// <summary>
    /// RpcHeader 协议说明
    /// RpcType(2 bit) + RpcRoute(14 bit) = short(16 bit)
    /// </summary>
    [ProtoContract]
    public class RpcResponse
    {
        [ProtoMember(1)]
        public bool Success { get; set; }
        [ProtoMember(2)]
        public byte[] Param { get; set; }
    }

    [ProtoContract]
    public class RpcReqeust
    {
        [ProtoMember(1)]
        public byte[] Param { get; set; }
    }

    [ProtoContract]
    public class RpcPush
    {
        [ProtoMember(1)]
        public byte[] Param { get; set; }
    }
}
