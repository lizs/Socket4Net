using System.Text;
using ProtoBuf;

namespace socket4net
{
    [ProtoContract]
    public class RpcPack
    {
        [ProtoMember(1)]
        public ERpc Type { get; set; }

        [ProtoMember(2)]
        public byte TargetNode { get; set; }

        [ProtoMember(3)]
        public short Ops { get; set; }

        [ProtoMember(4)]
        public long ObjId { get; set; }

        [ProtoMember(5)]
        public short ComponentId { get; set; }

        [ProtoMember(6)]
        public bool Succes { get; set; }

        [ProtoMember(7)]
        public byte[] Data { get; set; }

        [ProtoMember(8)]
        public ushort Serial { get; set; }

        [ProtoMember(9)]
        public long PlayerId { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("Type : " + Type);
            sb.AppendLine("TargetNode : " + TargetNode);
            sb.AppendLine("Ops : " + Ops);
            sb.AppendLine("ObjId : " + ObjId);
            sb.AppendLine("ComponentId : " + ComponentId);
            sb.AppendLine("Success : " + Succes);
            sb.AppendLine("Serial : " + Serial);
            sb.AppendLine("PlayerId : " + PlayerId);

            return sb.ToString();
        }
    }

    public class RpcRequest
    {
        public byte TargetNode { get; set; }
        public long PlayerId { get; set; }
        public short Ops { get; set; }
        public long ObjId { get; set; }
        public short ComponentId { get; set; }
        public byte[] Data { get; set; }

        public override string ToString()
        {
            return string.Format("请求[{0}:{1}:{2}]", PlayerId, Ops, ComponentId);
        }
    }

    public class RpcResponse
    {
        public bool Succes { get; set; }
        public byte[] Data { get; set; }
    }

    public class RpcPush
    {
        public byte TargetNode { get; set; }
        public short Ops { get; set; }
        public long ObjId { get; set; }
        public long PlayerId { get; set; }
        public short ComponentId { get; set; }
        public byte[] Data { get; set; }

        public override string ToString()
        {
            return string.Format("推送[{0}:{1}:{2}]", PlayerId, Ops, ComponentId);
        }
    }
}
