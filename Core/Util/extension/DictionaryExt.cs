using System;
using System.Collections.Generic;
using System.Linq;

namespace socket4net
{
    /// <summary>
    ///     extension for dictionary
    /// </summary>
    public static class DictionaryExt
    {
        /// <summary>
        ///     随机获取字典的值
        /// </summary>
        /// <param name="input"></param>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <returns></returns>
        public static TValue NextValue<TKey, TValue>(this Dictionary<TKey, TValue> input)
        {
            return input.NextPair().Value;
        }

        /// <summary>
        ///     随机获取字典的键值对
        /// </summary>
        /// <param name="input"></param>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static KeyValuePair<TKey, TValue> NextPair<TKey, TValue>(this Dictionary<TKey, TValue> input)
        {
            if (input.IsNullOrEmpty())
                throw new Exception("字典空，无法随机");

            var cnt = input.Count;
            var idx = Rand.Next(cnt);
            return input.ElementAt(idx);
        }

        /// <summary>
        ///     字典是否为空
        /// </summary>
        /// <param name="input"></param>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <returns></returns>
        public static bool IsNullOrEmpty<TKey, TValue>(this Dictionary<TKey, TValue> input)
        {
            return input == null || input.Count == 0;
        }

        /// <summary>
        ///     获取字典的值，若不存在则返回默认值（值类型默认值）
        /// </summary>
        /// <param name="input"></param>
        /// <param name="key"></param>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <returns></returns>
        public static TValue GetOrReturnDefault<TKey, TValue>(this Dictionary<TKey, TValue> input, TKey key)
        {
            if (input == null || !input.ContainsKey(key)) return default(TValue);
            return input[key];
        }

        /// <summary>
        ///     将字典转成对列表
        /// </summary>
        /// <param name="input"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<Pair<T>> ToListPair<T>(this Dictionary<T, T> input)
        {
            return input.Select(kv => new Pair<T>(kv.Key, kv.Value)).ToList();
        }

        /// <summary>
        ///     将字典转成列表（键值依次展开）
        /// </summary>
        /// <param name="input"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<T> ToList<T>(this Dictionary<T, T> input)
        {
            var ret = new List<T>();
            foreach (var kv in input)
            {
                ret.Add(kv.Key);
                ret.Add(kv.Value);
            }

            return ret;
        }

        /// <summary>
        ///     获取字典指定键对应的值
        /// </summary>
        /// <param name="input"></param>
        /// <param name="key"></param>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static TValue Get<TKey, TValue>(this Dictionary<TKey, TValue> input, TKey key)
        {
            if (input == null)
            {
                throw new ArgumentException();
            }

            if (!input.ContainsKey(key))
            {
                throw new ArgumentException();
            }

            return input[key];
        }

        /// <summary>
        ///     设置键值
        /// </summary>
        /// <param name="input"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        public static void Set<TKey, TValue>(this Dictionary<TKey, TValue> input, TKey key, TValue value)
        {
            if (input.ContainsKey(key))
            {
                input[key] = value;
                return;
            }

            input.Add(key, value);
        }

        /// <summary>
        ///     若指定键不存在，则添加值，并返回true，否则返回false
        /// </summary>
        /// <param name="input"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <returns></returns>
        public static bool AddIfNotExist<TKey, TValue>(this Dictionary<TKey, TValue> input, TKey key, TValue value)
        {
            if (input.ContainsKey(key)) return false;

            input.Add(key, value);
            return true;
        }

        /// <summary>
        ///     融合两个字典
        /// </summary>
        /// <param name="input"></param>
        /// <param name="pair"></param>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <returns></returns>
        public static Dictionary<TKey, TValue> Merge<TKey, TValue>(this Dictionary<TKey, TValue> input, Pair<TKey, TValue> pair)
        {
            Merge(input, pair.Key, pair.Value);
            return input;
        }

        /// <summary>
        ///     将对插入字典
        /// </summary>
        /// <param name="input"></param>
        /// <param name="pair"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Dictionary<T, T> Merge<T>(this Dictionary<T, T> input, Pair<T> pair)
        {
            if (pair != null)
                Merge(input, pair.Key, pair.Value);
            return input;
        }

        /// <summary>
        ///     将对列表融合进字典
        /// </summary>
        /// <param name="input"></param>
        /// <param name="pairs"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Dictionary<T, T> Merge<T>(this Dictionary<T, T> input, List<Pair<T>> pairs)
        {
            if (!pairs.IsNullOrEmpty())
            {
                foreach (var pair in pairs)
                {
                    input.Merge(pair);
                }
            }

            return input;
        }

        /// <summary>
        ///     将键值插入字典
        /// </summary>
        /// <param name="input"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <returns></returns>
        public static Dictionary<TKey, TValue> Merge<TKey, TValue>(this Dictionary<TKey, TValue> input, TKey key,
            TValue value)
        {
            if (AddIfNotExist(input, key, value)) return input;
            input[key] = Add<TValue>.Function(input[key], value);
            return input;
        }

        /// <summary>
        ///     融合两个字典
        /// </summary>
        /// <param name="input"></param>
        /// <param name="other"></param>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <returns></returns>
        public static Dictionary<TKey, TValue> Merge<TKey, TValue>(this Dictionary<TKey, TValue> input,
            Dictionary<TKey, TValue> other)
        {
            foreach (var kv in other)
            {
                input.Merge(kv.Key, kv.Value);
            }

            return input;
        }
    }
}
