using System;
using System.Collections.Generic;
using System.Linq;

namespace socket4net
{
    public static class ComponentIdCache
    {
        private static readonly Dictionary<Type, short> Cache = new Dictionary<Type, short>();

        public static short Get(Type type)
        {
            if (Cache.ContainsKey(type))
            {
                return Cache[type];
            }

            var attribute =
                type.GetCustomAttributes(typeof (ComponentIdAttribute), false)
                    .Cast<ComponentIdAttribute>()
                    .FirstOrDefault();
            if (attribute == null)
                throw new Exception(string.Format("目标组件{0}不包含 ComponentIdAttribute 属性", type.FullName));

            Cache[type] = attribute.Id;
            return attribute.Id;
        }

        public static void Clear()
        {
            Cache.Clear();
        }
    }

    public class ComponentIdAttribute : Attribute
    {
        public short Id { get; private set; }
        public ComponentIdAttribute(short id)
        {
            Id = id;
        }
    }

    public class ComponentsMgr<TPKey> : UniqueMgr<short, Component<TPKey>>
    {
        public T AddComponent<T>() where T : Component<TPKey>, new()
        {
            var id = ComponentIdCache.Get(typeof (T));
            var cp = Get(id);
            if (cp == null) 
                return Create<T>(new ComponentArg(this, id), false, false);

            throw new Exception(string.Format("Component already added : {0}", cp.GetType()));
        }
    }
}
