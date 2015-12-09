using ProtoBuf;
using socket4net;

namespace Proto
{
    public enum ECommand
    {
        Request,
        Push,
    }

    [ProtoContract]
    public class RequestMsgProto : IProtobufInstance
    {
        [ProtoMember(1)]
        public string Message { get; set; }
    }

    [ProtoContract]
    public class ResponseMsgProto : IProtobufInstance
    {
        [ProtoMember(1)]
        public string Message { get; set; }
    }

    [ProtoContract]
    public class PushMsgProto : IProtobufInstance
    {
        [ProtoMember(1)]
        public string Message { get; set; }
        [ProtoMember(2)]
        public string From { get; set; }
    }
}