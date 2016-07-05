using System;
using System.Collections.Generic;

namespace socket4net
{
    /// <summary>
    /// 不确定个数
    /// </summary>
    public class Indefinite<T> : IParsableFromString
    {
        public Indefinite(string str)
        {
            ParseFromString(str);
        }

        private List<T> _members = new List<T>();
        public List<T> Members
        {
            get { return _members; }
            set { _members = value; }
        }

        public T First { get { return _members[0]; } }
        public T Last { get { return _members[_members.Count - 1]; } }

        public List<T> Range(int offset, int length)
        {
            if (length > _members.Count) return null;

            var lst = new List<T>(length);
            for (var i = offset; i < length; i++)
                lst.Add(_members[i]);

            return lst;
        }

        private void ParseFromString(string str)
        {
            var array = str.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var x in array)
            {
                _members.Add((T)Convert.ChangeType(x, typeof(T)));
            }
        }
    }
}
