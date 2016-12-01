#region MIT

//  /*The MIT License (MIT)
// 
//  Copyright 2016 lizs lizs4ever@163.com
//  
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//  
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
//  
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
//   * */

#endregion

using System;
using System.Linq.Expressions;

namespace socket4net
{
    /// <summary>
    ///     - 运算
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class Subtract<T>
    {
        private static bool _compiled;
        private static Func<T, T, T> _function;

        /// <summary>
        ///     获取编译过的减运算
        /// </summary>
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

            return Expression.Lambda<Func<T, T, T>>(addExp, px, py).Compile();
        }
    }
}