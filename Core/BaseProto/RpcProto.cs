using ProtoBuf;

namespace Core.BaseProto
{
    /// <summary>
    /// RpcHeader 协议说明
    /// RpcType(3 bit) + RpcRoute(15 bit) = Short(18 bit)
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
    public class RpcNotify
    {
        [ProtoMember(1)]
        public byte[] Param { get; set; }
    }
}
