using System;
using System.Collections.Generic;
using System.Linq;

namespace socket4net
{
    /*  todo lizs
     *  在方法中AddComponent更直观
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class ConsistsOfAttribute : Attribute
    {
        public Type[] Components { get; private set; }
        public ConsistsOfAttribute(params Type[] components)
        {
            Components = components;
        }
    }

    public class ComponentsCache : AttributesCache<Type[], ConsistsOfAttribute>
    {
        private static ComponentsCache _ins;
        public static ComponentsCache Ins
        {
            get { return _ins ?? (_ins = new ComponentsCache()); }
        }

        protected override Type[] Handle(IEnumerable<ConsistsOfAttribute> attributes)
        {
            var dependOnAttributes = attributes as ConsistsOfAttribute[] ?? attributes.ToArray();
            return dependOnAttributes.IsNullOrEmpty()
                ? new Type[] { }
                : dependOnAttributes.SelectMany(x => x.Components).ToArray();
        }

        protected override Type[] Handle(ConsistsOfAttribute attribute)
        {
            throw new NotImplementedException("");
        }
    }
     * */
}