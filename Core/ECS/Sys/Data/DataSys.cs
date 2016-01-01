using System;
using System.Collections.Generic;

namespace socket4net
{
    public class DataSysArg : ObjArg
    {
        public DataSysArg(IObj owner, EntitySys es) : base(owner)
        {
            Es = es;
        }

        public EntitySys Es { get; private set; }
    }

    /// <summary>
    ///     数据系统
    /// </summary>
    public abstract class DataSys : Obj
    {
        private EntitySys _es;
        protected readonly Dictionary<Guid, List<IBlock>> UpdateCache = new Dictionary<Guid, List<IBlock>>();
        protected readonly Dictionary<Guid, Type> DestroyCache = new Dictionary<Guid, Type>(); 

        protected override void OnInit(ObjArg arg)
        {
            base.OnInit(arg);

            _es = arg.As<DataSysArg>().Es;

            // 监听实体系统
            _es.GlobalListen(OnEntityPropertyChanged);
            _es.EventDefaultObjCreated += OnEntityCreated;
            _es.EventObjDestroyed += OnEntityDestroyed;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _es.GlobalUnlisten(OnEntityPropertyChanged);
            _es.EventDefaultObjCreated -= OnEntityCreated;
            _es.EventObjDestroyed -= OnEntityDestroyed;
        }

        private void OnEntityCreated(Entity entity)
        {
            CacheBlock(entity.Id, entity.Blocks);
        }

        private void OnEntityDestroyed(Guid id, Type type)
        {
            if(!DestroyCache.ContainsKey(id))
                DestroyCache.Add(id, type);

            UpdateCache.Remove(id);
        }

        private void CacheBlock(Guid id, IReadOnlyCollection<IBlock> blocks)
        {
            if(blocks.IsNullOrEmpty()) return;
            foreach (var block in blocks)
            {
                CacheBlock(id, block);
            }
        }

        private void CacheBlock(Guid id, IBlock block)
        {
            if (!UpdateCache.ContainsKey(id))
                UpdateCache[id] = new List<IBlock> { block };
            else
            {
                var lst = UpdateCache[id];
                if (lst.Contains(block)) return;
                lst.Add(block);
            }
        }

        private void OnEntityPropertyChanged(Entity entity, IBlock block)
        {
            CacheBlock(entity.Id, block);
        }
    }
}
