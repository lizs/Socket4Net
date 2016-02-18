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
        protected EntitySys Es;
        protected readonly Dictionary<long, List<IBlock>> UpdateCache = new Dictionary<long, List<IBlock>>();
        protected readonly Dictionary<long, Type> DestroyCache = new Dictionary<long, Type>(); 

        protected override void OnInit(ObjArg arg)
        {
            base.OnInit(arg);

            Es = arg.As<DataSysArg>().Es;

            // 监听实体系统
            Es.GlobalListen(OnEntityPropertyChanged);
            Es.EventDefaultObjCreated += OnEntityCreated;
            Es.EventObjDestroyed += OnEntityDestroyed;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Es.GlobalUnlisten(OnEntityPropertyChanged);
            Es.EventDefaultObjCreated -= OnEntityCreated;
            Es.EventObjDestroyed -= OnEntityDestroyed;
        }

        private void OnEntityCreated(Entity entity)
        {
            CacheBlock(entity.Id, entity.Blocks);
        }

        private void OnEntityDestroyed(long id, Type type)
        {
            if(!DestroyCache.ContainsKey(id))
                DestroyCache.Add(id, type);

            UpdateCache.Remove(id);
        }

        private void CacheBlock(long id, IReadOnlyCollection<IBlock> blocks)
        {
            if(blocks.IsNullOrEmpty()) return;
            foreach (var block in blocks)
            {
                CacheBlock(id, block);
            }
        }

        private void CacheBlock(long id, IBlock block)
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
