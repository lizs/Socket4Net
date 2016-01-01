using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace socket4net
{
    public class StringParser
    {
        /// <summary>
        ///     根据属性信息从字符串解析出一个对象
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        public static object ParseValue(PropertyInfo propertyInfo, string str)
        {
            object value;
            if (typeof(IParseFromString).IsAssignableFrom(propertyInfo.PropertyType))
            {
                value = ParseFromString(str, propertyInfo.PropertyType);
            }
            else if (propertyInfo.PropertyType == typeof(Type))
            {
                value = Type.GetType(str, false, true);
            }
            else if (propertyInfo.PropertyType == typeof(TimeSpan))
            {
                value = string.IsNullOrEmpty(str)
                    ? TimeSpan.Zero
                    : TimeSpan.Parse(str);
            }
            else if (typeof(IList).IsAssignableFrom(propertyInfo.PropertyType))
            {
                var args = propertyInfo.PropertyType.GetGenericArguments();
                value = ParseListFromString(str, args[0], new[] { '+' });
            }
            else if (propertyInfo.PropertyType.IsEnum)
            {
                value = string.IsNullOrEmpty(str)
                    ? DefaultTraitor.GetDefault(propertyInfo.PropertyType)
                    : Enum.Parse(propertyInfo.PropertyType, str, true);
            }
            else
            {
                value = string.IsNullOrEmpty(str)
                    ? DefaultTraitor.GetDefault(propertyInfo.PropertyType)
                    : Convert.ChangeType(str, propertyInfo.PropertyType);
            }
            return value;
        }

        public static object ParseValue(Type type, string str)
        {
            object value;
            if (typeof(IParseFromString).IsAssignableFrom(type))
            {
                value = ParseFromString(str, type);
            }
            else if (type == typeof(TimeSpan))
            {
                value = string.IsNullOrEmpty(str)
                    ? TimeSpan.Zero
                    : TimeSpan.Parse(str);
            }
            else if (type.IsEnum)
            {
                value = string.IsNullOrEmpty(str)
                    ? DefaultTraitor.GetDefault(type)
                    : Enum.Parse(type, str, true);
            }
            else
            {
                value = string.IsNullOrEmpty(str)
                    ? DefaultTraitor.GetDefault(type)
                    : Convert.ChangeType(str, type);
            }
            return value;
        }

        public static T ParseFromString<T>(string str) where T : IParseFromString, new()
        {
            if (string.IsNullOrEmpty(str))
                return default(T);

            var ret = new T();
            ret.ParseFromString(str);
            return ret;
        }

        public static object ParseFromString(string str, Type type)
        {
            if (string.IsNullOrEmpty(str))
                return DefaultTraitor.GetDefault(type);

            var ci = type.GetConstructor(new[] { typeof(string) });
            return ci.Invoke(new object[] { str });
        }

        public static List<int> ParseFromString(string strInput, char[] separator)
        {
            if (string.IsNullOrEmpty(strInput))
                return null;

            var arr = strInput.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            return arr.Select(int.Parse).ToList();
        }

        public static List<T> ParseFromString<T>(string strInput, char[] separator) where T : IParseFromString, new()
        {
            if (string.IsNullOrEmpty(strInput))
                return null;

            var arr = strInput.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            var ls = new List<T>();
            foreach (var str in arr)
            {
                var t = new T();
                t.ParseFromString(str);
                ls.Add(t);
            }

            return ls;
        }

        public static object ParseListFromString(string strInput, Type type, char[] separator)
        {
            if (string.IsNullOrEmpty(strInput))
                return null;

            var arr = strInput.Split(separator, StringSplitOptions.RemoveEmptyEntries);

            var unkonwn = typeof(List<>);
            Type[] typeArgs = { type };
            var generic = unkonwn.MakeGenericType(typeArgs);

            var constructor = generic.GetConstructor(Type.EmptyTypes);
            var list = constructor.Invoke(null);

            foreach (var t in arr.Select(str => ParseValue(type, str)))
            {
                (list as IList).Add(t);
            }

            return list;
        }
    }
}
