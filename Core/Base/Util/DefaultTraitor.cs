using System;

namespace socket4net
{
    public interface IDefault
    {
        object Value { get; }
    }

    public class Default<T> : IDefault
    {
        public object Value
        {
            get { return default(T); }
        }
    }

    public class DefaultTraitor
    {
        public static object GetDefault(Type type)
        {
            if (type == typeof(string)) return string.Empty;

            var unkonwn = typeof(Default);
            Type[] typeArgs = { type };
            var generic = unkonwn.MakeGenericType(typeArgs);

            var constructor = generic.GetConstructor(Type.EmptyTypes);
            var ret = constructor.Invoke(null);

            return (ret as IDefault).Value;
        }
    }
}
