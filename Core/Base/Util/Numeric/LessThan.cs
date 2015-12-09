using System;
using System.Linq.Expressions;

namespace socket4net
{
    public static class LessThan<T>
    {
        private static bool _compiled;
        private static Func<T, T, bool> _function;
        public static Func<T, T, bool> Function
        {
            get
            {
                if (_compiled) return _function;
                _function = Compile();
                _compiled = true;
                return _function;
            }
        }

        private static Func<T, T, bool> Compile()
        {
            var px = Expression.Parameter(typeof(T), "x");
            var py = Expression.Parameter(typeof(T), "y");
            var addExp = Expression.LessThan(px, py);

            return Expression.Lambda<Func<T, T, bool>>(addExp, new[] { px, py }).Compile();
        }
    }

    public static class LessThanOrEqual<T>
    {
        private static bool _compiled;
        private static Func<T, T, bool> _function;
        public static Func<T, T, bool> Function
        {
            get
            {
                if (_compiled) return _function;
                _function = Compile();
                _compiled = true;
                return _function;
            }
        }

        private static Func<T, T, bool> Compile()
        {
            var px = Expression.Parameter(typeof(T), "x");
            var py = Expression.Parameter(typeof(T), "y");
            var addExp = Expression.LessThanOrEqual(px, py);

            return Expression.Lambda<Func<T, T, bool>>(addExp, new[] { px, py }).Compile();
        }
    }
}