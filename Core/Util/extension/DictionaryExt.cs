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
        public static TValue NextValue<TKey, TValue>(this Dictionary<TKey, TValue> input)
        {
            return input.NextPair().Value;
        }

        public static KeyValuePair<TKey, TValue> NextPair<TKey, TValue>(this Dictionary<TKey, TValue> input)
        {
            if (input.IsNullOrEmpty())
                throw new Exception("字典空，无法随机");

            var cnt = input.Count;
            var idx = Rand.Next(cnt);
            return input.ElementAt(idx);
        }

        public static bool IsNullOrEmpty<TKey, TValue>(this Dictionary<TKey, TValue> input)
        {
            return input == null || input.Count == 0;
        }

        public static TValue GetOrReturnDefault<TKey, TValue>(this Dictionary<TKey, TValue> input, TKey key)
        {
            if (input == null || !input.ContainsKey(key)) return default(TValue);
            return input[key];
        }

        public static List<Pair<T>> ToListPair<T>(this Dictionary<T, T> input)
        {
            return input.Select(kv => new Pair<T>(kv.Key, kv.Value)).ToList();
        }

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

        public static TValue Get<TKey, TValue>(this Dictionary<TKey, TValue> input, TKey key, string title = "")
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

        public static void Set<TKey, TValue>(this Dictionary<TKey, TValue> input, TKey key, TValue value)
        {
            if (input.ContainsKey(key))
            {
                input[key] = value;
                return;
            }

            input.Add(key, value);
        }

        public static bool AddIfNotExist<TKey, TValue>(this Dictionary<TKey, TValue> input, TKey key, TValue value)
        {
            if (input.ContainsKey(key)) return false;

            input.Add(key, value);
            return true;
        }

        public static Dictionary<TKey, TValue> Merge<TKey, TValue>(this Dictionary<TKey, TValue> input, Pair<TKey, TValue> pair)
        {
            Merge(input, pair.Key, pair.Value);
            return input;
        }

        public static Dictionary<T, T> Merge<T>(this Dictionary<T, T> input, Pair<T> pair)
        {
            if (pair != null)
                Merge(input, pair.Key, pair.Value);
            return input;
        }

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

        public static Dictionary<TKey, TValue> Merge<TKey, TValue>(this Dictionary<TKey, TValue> input, TKey key,
            TValue value)
        {
            if (AddIfNotExist(input, key, value)) return input;
            input[key] = Add<TValue>.Function(input[key], value);
            return input;
        }

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
