using System;
using ProtoBuf;

namespace socket4net
{
    public class Treble<T> : Treble<T, T, T>
    {
        public Treble(string str) : base(str)
        {
        }

        public Treble(T one, T two, T three)
            : base(one, two, three)
        {
        }
    }

    /// <summary>
    ///     用于解析诸如 1 2 4 + 3 3 5 + 7 6 9 这种类型的string
    /// </summary>
    [ProtoContract]
    public class Treble<T1, T2, T3> : IParsableFromString
    {
        public Treble(string str)
        {
            Parse(str);
        }

        public Treble(T1 one, T2 two, T3 three)
        {
            One = one;
            Two = two;
            Three = three;
        }

        [ProtoMember(1)]
        public T1 One { get; set; }

        [ProtoMember(2)]
        public T2 Two { get; set; }

        [ProtoMember(3)]
        public T3 Three { get; set; }

        private void Parse(string str)
        {
            var x = str.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            if (x.Length != 3)
            {
                throw new IndexOutOfRangeException("[Treble # ParseFromString]:数组大小需要为3");
            }

            One = x[0].To<T1>();
            Two = x[1].To<T2>();
            Three = x[2].To<T3>();
        }
    }
}