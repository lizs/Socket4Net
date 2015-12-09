using System;
using System.Linq.Expressions;

namespace socket4net
{
    public static class Subtract<T>
    {
        private static bool _compiled;
        private static Func<T, T, T> _function;
        public static Func<T, T, T> Function
        {
            get
            {
                if (_compiled) return _function;
                _function = Compile();
                _compiled = true;
                return _function;
            }
        }

        private static Func<T, T, T> Compile()
        {
            var px = Expression.Parameter(typeof(T), "x");
            var py = Expression.Parameter(typeof(T), "y");
            var addExp = Expression.Subtract(px, py);

            return Expression.Lambda<Func<T, T, T>>(addExp, new[] { px, py }).Compile();
        }
    }
}