using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;

namespace socket4net
{
    [ProtoContract]
    public class ProtoWrapper<T>
    {
        [ProtoMember(1)]
        public T Item { get; set; }
    }

    /// <summary>
    ///     对接Redis key-value
    /// </summary>
    public class RedisEntry
    {
        public string Feild { get; set; }
        public byte[] Data { get; set; }
    }

    /// <summary>
    ///     实体信息
    ///     一个实体由 类型、Id、属性定义
    /// </summary>
    public class EntityEntry
    {
        public Type Type { get; set; }
        public Guid Id { get; set; }
        public IReadOnlyCollection<RedisEntry> Blocks { get; set; }
    }

    /// <summary>
    ///     存储系统（针对Redis）
    /// </summary>
    public class PersistSys : DataSys
    {
        private bool _busy = false;
        private Dictionary<Guid, List<IBlock>> _tmpUpdateCache;
        private Dictionary<Guid, Type> _tmpDestroyCache; 
        
        /// <summary>
        ///     存储
        /// </summary>
        public void PersistAsync(IAsyncRedisClient client)
        {
            if(_busy) return;
            _busy = true;

            // 拷贝缓存
            _tmpDestroyCache = DestroyCache.ToDictionary(kv => kv.Key, kv => kv.Value);
            _tmpUpdateCache = UpdateCache.ToDictionary(kv => kv.Key, kv => kv.Value.ToList());

            // 删除
            client.HashMultiDelAsync("", )

            // 存储
        }
    }
}
