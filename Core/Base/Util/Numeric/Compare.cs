using System;
using System.Linq.Expressions;

namespace Pi.Common.Numeric
{
    public static class Compare<T>
    {
        private static bool _compiled;
        private static Func<T, T, int> _function;
        public static Func<T, T, int> Function
        {
            get
            {
                if (_compiled) return _function;
                _function = Compile();
                _compiled = true;
                return _function;
            }
        }

        private static Func<T, T, int> Compile()
        {
            var px = Expression.Parameter(typeof(T), "x");
            var py = Expression.Parameter(typeof(T), "y");
            var addExp = Expression.Equal(px, py);

            return Expression.Lambda<Func<T, T, int>>(addExp, new[] { px, py }).Compile();
        }
    }
}