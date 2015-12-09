using System.Collections.Generic;
using ProtoBuf;

namespace socket4net
{
    public interface IListOpsProto : IProtobufInstance
    {
        byte[] Serialize();
    }

    /// <summary>
    ///     移除单个
    ///     按照Id移除
    /// </summary>
    public class RemoveOpsProto : IListOpsProto
    {
        /// <summary>
        ///     要移除的Id
        /// </summary>
        public int Id { get; set; }

        public byte[] Serialize()
        {
            return PiSerializer.SerializeValue(Id);
        }

        public static RemoveOpsProto Deserialize(byte[] bytes)
        {
            return new RemoveOpsProto {Id = PiSerializer.DeserializeValue<int>(bytes)};
        }
    }

    /// <summary>
    ///     插入
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class InsertOpsProto<T> : IListOpsProto
    {
        [ProtoContract]
        public class InsertProto : IProtobufInstance
        {
            [ProtoMember(1)]
            public int Index { get; set; }

            [ProtoMember(2)]
            public ListItemRepresentation<T> ListItem { get; set; }
        }

        public ListItemRepresentation<T> ListItem { get; set; }
        public int Index { get; set; }

        public byte[] Serialize()
        {
            return PiSerializer.Serialize(new InsertProto {Index = Index, ListItem = ListItem});
        }

        public static InsertOpsProto<T> Deserialize(byte[] bytes)
        {
            var proto = PiSerializer.Deserialize<InsertProto>(bytes);
            return new InsertOpsProto<T> {Index = proto.Index, ListItem = proto.ListItem};
        }
    }

    /// <summary>
    ///     更新
    /// </summary>
    public class UpdateOpsProto<T> : IListOpsProto
    {
        public ListItemRepresentation<T> ListItem { get; set; }

        public byte[] Serialize()
        {
            return PiSerializer.SerializeValue(ListItem);
        }

        public static UpdateOpsProto<T> Deserialize(byte[] bytes)
        {
            return new UpdateOpsProto<T> {ListItem = PiSerializer.DeserializeValue<ListItemRepresentation<T>>(bytes)};
        }
    }

    /// <summary>
    ///     列表操作协议
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [ProtoContract]
    public class ListOpsProto<T> : IProtobufInstance
    {
        [ProtoMember(1)]
        public List<KeyValuePair<EPropertyOps, byte[]>> OpsStack { get; set; }
    }
}
