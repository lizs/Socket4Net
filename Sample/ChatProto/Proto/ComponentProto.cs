using ProtoBuf;

namespace Shared
{
    public enum EChatOps
    {
        Echo,
        Broadcst,

        Create,
        Destroy,
    }
    
    [ProtoContract]
    public class EchoProto
    {
        [ProtoMember(1)]
        public string Message { get; set; }
    }

    [ProtoContract]
    public class EchoResponseProto
    {
        [ProtoMember(1)]
        public string Message { get; set; }
    }

    [ProtoContract]
    public class BroadcastProto
    {
        [ProtoMember(1)]
        public string Message { get; set; }
    }
}
