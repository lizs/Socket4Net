using System;
using System.Collections.Generic;
using System.Linq;

namespace socket4net
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class DependOnAttribute : Attribute
    {
        public Type[] Targets { get; private set; }
        public DependOnAttribute(params Type[]targets)
        {
            Targets = targets.ToArray();
        }
    }

    public class ComponentDepedencyCache : AttributesCache<Type[], DependOnAttribute>
    {
        private static ComponentDepedencyCache _instance;
        public static ComponentDepedencyCache Instance
        {
            get { return _instance ?? (_instance = new ComponentDepedencyCache()); }
        }

        protected override Type[] Handle(IEnumerable<DependOnAttribute> attributes)
        {
            var dependOnAttributes = attributes as DependOnAttribute[] ?? attributes.ToArray();
            return dependOnAttributes.IsNullOrEmpty()
                ? new Type[] {}
                : dependOnAttributes.SelectMany(x => x.Targets).ToArray();
        }

        protected override Type[] Handle(DependOnAttribute attribute)
        {
            throw new NotImplementedException("");
        }
    }
}