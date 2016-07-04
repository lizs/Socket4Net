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
using System.Collections.Generic;
using System.Linq;

namespace socket4net
{
    /// <summary>
    ///     extension for LINQ
    /// </summary>
    public static class LinqExt
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="enumerable"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null)
                return true;

            var collection = enumerable as ICollection<T>;
            if (collection != null)
                return collection.Count == 0;

            return !enumerable.Any();
        }

        /// <summary>
        ///     分组,对分
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <param name="parts"></param>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> input, int parts)
        {
            return input.Select((item, index) => new { index, item })
                       .GroupBy(x => x.index % parts)
                       .Select(x => x.Select(y => y.item));
        }

        /// <summary>
        ///     get next random item from input
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        public static T NextRand<T>(this IEnumerable<T> input)
        {
            if (input.IsNullOrEmpty())
            {
                throw new Exception("IEnumerable为空，无法随机");
            }

            var cnt = input.Count();
            var idx = Rand.Next(cnt);
            return input.ElementAt(idx);
        }
    }
}
