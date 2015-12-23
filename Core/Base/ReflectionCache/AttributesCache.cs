using System;
using System.Collections.Generic;
using System.Linq;

namespace socket4net
{
    public abstract class AttributesCache<TValue, TAttribute> where TAttribute : Attribute
    {
        private readonly Dictionary<Type, TValue> _cache = new Dictionary<Type, TValue>();

        public TValue Get(Type type)
        {
            if (_cache.ContainsKey(type))
                return _cache[type];

            var usage =
                (AttributeUsageAttribute)
                    typeof (TAttribute).GetCustomAttributes(typeof (AttributeUsageAttribute), false).FirstOrDefault();

            var allowMultiple = usage == null || usage.AllowMultiple;
            var inherited = usage == null || usage.Inherited;

            var attributes = type.GetCustomAttributes(typeof(TAttribute), inherited)
                .Cast<TAttribute>();

            var ret = allowMultiple ? Handle(attributes) : Handle(attributes.FirstOrDefault());
            _cache[type] = ret;

            return ret;
        }

        protected abstract TValue Handle(IEnumerable<TAttribute> attributes);
        protected abstract TValue Handle(TAttribute attribute);

        public void Clear()
        {
            _cache.Clear();
        }
    }
}