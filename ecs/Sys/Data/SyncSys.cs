using System.Collections.Generic;
using System.Linq;
using ProtoBuf;
using socket4net;

namespace ecs
{
    [ProtoContract]
    public class BlockProto
    {
        [ProtoMember(1)]
        public short Pid { get; set; }
        [ProtoMember(2)]
        public byte[] Data { get; set; }
    }

    [ProtoContract]
    [ProtoInclude(10, typeof(EntityDestroyProto))]
    [ProtoInclude(20, typeof(EntityUpdateProto))]
    public class EntityProto
    {
        [ProtoMember(1)]
        public long Id { get; set; }
    }

    [ProtoContract]
    public class EntityDestroyProto : EntityProto
    {
    }
    
    [ProtoContract]
    public class EntityUpdateProto : EntityProto
    {
        [ProtoMember(1)]
        public string Type { get; set; }
        [ProtoMember(2)]
        public List<BlockProto> Blocks { get; set; }
    }

    ///     同步系统
    ///     即时批量同步
    ///     注：同步实体数据到对端，亦即对端实体数据是本端实体系统的完全克隆
    public class SyncSys : DataSys
    {
        /// <summary>
        ///     在指定会话中同步实体数据
        /// </summary>
        /// <param name="session"></param>
        public void Sync(IRpcSession session)
        {
            Sync(new[] {session});
        }

        /// <summary>
        ///     在指定会话中同步实体数据
        /// </summary>
        public void Sync(IEnumerable<IRpcSession> sessions)
        {
            var proto = new List<EntityProto>();

            // 更新
            proto.AddRange(UpdateCache.Select(
                x =>
                    new EntityUpdateProto
                    {
                        Id = x.Key,
                        Type = x.Value.Key.FullName,
                        Blocks = x.Value.Value.Select(y => new BlockProto { Pid = y.Id, Data = y.Serialize() }).ToList()
                    }));

            // 销毁
            proto.AddRange(DestroyCache.Select(x => new EntityDestroyProto { Id = x.Key }));

            // 同步
            if (!proto.IsNullOrEmpty())
            {
                var player = GetAncestor<Player>();
                foreach (var rpcSession in sessions)
                {
                    rpcSession.Push(0, player.Id, (short)ESyncOps.Sync,
                        new ProtoWrapper<List<EntityProto>> { Item = proto }, 0, (short)EInternalComponentId.Sync);
                }
            }

            UpdateCache.Clear();
            DestroyCache.Clear();
        }
    }
}
