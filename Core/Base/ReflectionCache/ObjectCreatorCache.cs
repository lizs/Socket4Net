using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace socket4net
{
    public static class ObjFactory
    {
        private static readonly Dictionary<Type, Func<Obj>> CtorCache = new Dictionary<Type, Func<Obj>>();

        private static Func<Obj> GetCtor(Type type)
        {
            if (CtorCache.ContainsKey(type)) return CtorCache[type];

            Expression body = Expression.New(type);
            var ret = Expression.Lambda<Func<Obj>>(body).Compile();
            CtorCache[type] = ret;
            return ret;
        }

        public static Obj Create(Type type, ObjArg arg)
        {
            var ctor = GetCtor(type);
            var obj = ctor();
            obj.Init(arg);
            return obj;
        }

        public static T Create<T>(ObjArg arg) where T : Obj
        {
            var obj = Create(typeof (T), arg);
            return (T) obj;
        }

        public static void ClearCtorCache()
        {
            CtorCache.Clear();
        }
    }
}
