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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace socket4net
{
    /// <summary>
    ///     
    /// </summary>
    public interface IParsableFromString
    {
    }

    /// <summary>
    /// 
    /// </summary>
    public static class Convertor
    {
        /// <summary>
        ///     get default value for type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object GetDefault(Type type)
        {
            return type.IsValueType
                ? Activator.CreateInstance(type)
                : null;
        }

        /// <summary>
        ///     get an empty list
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IList GetEmptyList(Type type)
        {
            var constructor = typeof(List<>).MakeGenericType(type).GetConstructor(Type.EmptyTypes);
            if (constructor == null) return null;
            return (IList)constructor.Invoke(null);
        }

        /// <summary>
        ///     generic parse an object from string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        public static T To<T>(this string input)
        {
            return (T) To(input, typeof (T));
        }

        /// <summary>
        ///     non-generic convertor
        /// </summary>
        /// <param name="str"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object To(this string str, Type type)
        {
            if (typeof(IList).IsAssignableFrom(type))
            {
                var args = type.GetGenericArguments();
                return ToList(str, args[0], '+');
            }

            if (str.IsNullOrWhiteSpace())
                return GetDefault(type);

            if (typeof(IParsableFromString).IsAssignableFrom(type))
            {
                var ci = type.GetConstructor(new[] { typeof(string) });
                return ci.Invoke(new object[] { str });
            }

            if (type == typeof(Type))
            {
                return Type.GetType(str, false, true);
            }

            if (type == typeof(TimeSpan))
            {
                return TimeSpan.Parse(str);
            }

            if (type.IsEnum)
                return Enum.Parse(type, str, true);
            
            return  Convert.ChangeType(str, type);
        }

        /// <summary>
        ///     parse string to list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static List<T> ToList<T>(string input, params char[] separator)
        {
            return (List<T>) ToList(input, typeof (T), separator);
        }

        /// <summary>
        ///     parse string to list
        /// </summary>
        /// <param name="strInput"></param>
        /// <param name="type"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static IList ToList(string strInput, Type type, params char[] separator)
        {
            var lst = GetEmptyList(type);
            if (strInput.IsNullOrWhiteSpace())
                return lst;

            var arr = strInput.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            foreach (var t in arr.Select(str => str.To(type)))
            {
                lst.Add(t);
            }

            return lst;
        }
    }
}
