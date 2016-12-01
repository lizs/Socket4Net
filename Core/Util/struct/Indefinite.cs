using System;
using System.Collections.Generic;

namespace socket4net
{
    /// <summary>
    ///     不确定个数
    /// </summary>
    public class Indefinite<T> : IParsableFromString
    {
        public Indefinite(string str)
        {
            ParseFromString(str);
        }

        public List<T> Members { get; set; } = new List<T>();

        public T First => Members[0];
        public T Last => Members[Members.Count - 1];

        public List<T> Range(int offset, int length)
        {
            if (length > Members.Count) return null;

            var lst = new List<T>(length);
            for (var i = offset; i < length; i++)
                lst.Add(Members[i]);

            return lst;
        }

        private void ParseFromString(string str)
        {
            var array = str.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            foreach (var x in array)
            {
                Members.Add((T) Convert.ChangeType(x, typeof(T)));
            }
        }
    }
}