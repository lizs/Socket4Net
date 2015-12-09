using ProtoBuf;

namespace socket4net
{
    /// <summary>
    ///     封装一个Item
    /// </summary>
    [ProtoContract]
    public class ListItemRepresentation<T>
    {
        /// <summary>
        ///     分配给Item的Id
        ///     在当前Block唯一
        /// </summary>
        [ProtoMember(1)]
        public int Id { get; set; }

        /// <summary>
        ///     目标Item
        /// </summary>
        [ProtoMember(2)]
        public T Item { get; set; }
    }
}