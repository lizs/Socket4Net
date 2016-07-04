using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace socket4net
{
    /// <summary>
    /// extension for list
    /// </summary>
    public static class ListExt
    {
        public static List<string> ToStringList<T>(this List<T> input)
        {
            return input.IsNullOrEmpty() ? null : input.Select(x => x.ToString()).ToList();
        }

        public static bool AddIfNotExist<T>(this List<T> input, T item)
        {
            if (input == null) return false;
            if (input.Contains(item)) return false;
            input.Add(item);
            return true;
        }

        public static List<Pair<T>> Mult<T>(this List<Pair<T>> input, int n)
        {
            if (!input.IsNullOrEmpty())
                input.ForEach(x => x.Value = Multiply<T>.Function(x.Value, n));
            return input;
        }

        public static bool Merge(this List<Pair<int>> input, Pair<int> item)
        {
            return Merge(input, new List<Pair<int>> { item });
        }

        public static bool Merge(this List<Pair<int>> input, List<Pair<int>> items)
        {
            if (input == null) return false;

            var dic = input.ToDic();
            input.Clear();
            input.AddRange(dic.Merge(items).ToListPair());
            return true;
        }

        public static List<Pair<T>> ToPair<T>(this List<T> input, int startIdx)
        {
            // 空、不足、不能被2整除
            if (input == null || input.Count <= startIdx || (input.Count - startIdx) % 2 != 0) return null;

            var ret = new List<Pair<T>>();
            for (var i = startIdx; i < input.Count; i += 2)
            {
                ret.Add(new Pair<T>(input[i], input[i + 1]));
            }

            return ret;
        }

        public static Dictionary<T, T> ToDic<T>(this List<Pair<T>> input)
        {
            var dic = new Dictionary<T, T>();
            input.ForEach(pair => dic.Merge(pair));
            return dic;
        }

        public static string Format<T>(this List<T> input)
        {
            var sb = new StringBuilder();
            var cnt = input.IsNullOrEmpty() ? 0 : input.Count;
            for (var i = 0; i < cnt; i++)
            {
                sb.Append(i < cnt - 1 ? input[i] + "+" : input[i].ToString());
            }

            return sb.ToString();
        }
    }
}
