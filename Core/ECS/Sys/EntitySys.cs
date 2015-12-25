using System;
using System.Collections.Generic;
using System.Linq;

namespace socket4net
{
    public class EntityMgr : UniqueMgr<string, Entity>
    {
    }

    public class EntitySysArg : ObjArg
    {
        public EntitySysArg(IObj owner, IEnumerable<Type> groups) : base(owner)
        {
            Groups = groups;
        }

        public IEnumerable<Type> Groups { get; private set; } 
    }

    /// <summary>
    ///     实体系统
    /// </summary>
    public class EntitySys : Obj
    {
        private readonly Dictionary<Type, EntityMgr> _groups = new Dictionary<Type, EntityMgr>();
        public EntityMgr this[Type group]
        {
            get { return _groups.ContainsKey(@group) ? _groups[@group] : null; }
        }

        protected override void OnInit(ObjArg arg)
        {
            base.OnInit(arg);

            var more = arg.As<EntitySysArg>();
            foreach (var @group in more.Groups.Where(@group => !_groups.ContainsKey(@group)))
            {
                _groups[@group] = Create<EntityMgr>(new UniqueMgrArg(this));
            }
        }

        protected override void OnStart()
        {
            base.OnStart();
            foreach (var @group in _groups)
            {
                group.Value.Start();
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            foreach (var @group in _groups)
            {
                group.Value.Destroy();
            }
        }
    }
}
