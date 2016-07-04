using System;
using System.Linq.Expressions;

namespace socket4net
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class Multiply<T>
    {
        private static bool _compiled;
        private static Func<T, int, T> _function;
        public static Func<T, int, T> Function
        {
            get
            {
                if (_compiled) return _function;
                _function = Compile();
                _compiled = true;
                return _function;
            }
        }

        private static Func<T, int, T> Compile()
        {
            var px = Expression.Parameter(typeof(T), "x");
            var py = Expression.Parameter(typeof(int), "y");
            var multiplyExp = Expression.Multiply(px, py);

            return Expression.Lambda<Func<T, int, T>>(multiplyExp, new[] { px, py }).Compile();
        }
    }
}
