using ProtoBuf;

namespace Proto
{
    public enum ECommand
    {
        Request,
        Push,
    }

    [ProtoContract]
    public class RequestMsgProto
    {
        [ProtoMember(1)]
        public string Message { get; set; }
    }

    [ProtoContract]
    public class ResponseMsgProto
    {
        [ProtoMember(1)]
        public string Message { get; set; }
    }

    [ProtoContract]
    public class PushMsgProto
    {
        [ProtoMember(1)]
        public string Message { get; set; }
        [ProtoMember(2)]
        public string From { get; set; }
    }
}