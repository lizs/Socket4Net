using ProtoBuf;

namespace Proto
{
    public enum RpcRoute : short
    {
        GmCmd,
        Chat
    }

    [ProtoContract]
    public class Broadcast2Clients
    {
        [ProtoMember(1)]
        public string Message { get; set; }
        [ProtoMember(2)]
        public string From { get; set; }
    }

    [ProtoContract]
    public class Message2Server
    {
        [ProtoMember(1)]
        public string Message { get; set; }
    }
}