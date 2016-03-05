#region MIT
//  /*The MIT License (MIT)
// 
//  Copyright 2016 lizs lizs4ever@163.com
//  
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//  
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
//  
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
//   * */
#endregion
using System.Collections.Generic;
using ProtoBuf;

namespace socket4net
{
    public interface IListOpsProto
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
        public class InsertProto
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
    public class ListOpsProto<T>
    {
        [ProtoMember(1)]
        public List<KeyValuePair<EPropertyOps, byte[]>> OpsStack { get; set; }
    }
}
