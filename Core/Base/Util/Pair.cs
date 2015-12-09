using ProtoBuf;

namespace socket4net.Util
{
    [ProtoContract]
    public class Pair<TFirst, TSecond>
    {
        public Pair() { }

        public Pair(TFirst key, TSecond value)
        {
            Key = key;
            Value = value;
        }
        
        [ProtoMember(1)]
        public TFirst Key { get; set; }
        [ProtoMember(2)]
        public TSecond Value { get; set; }

        public override string ToString()
        {
            return string.Format("{0}:{1}", Key, Value);
        }
    }

    [ProtoContract]
    public class Pair<T> : Pair<T, T>
    {
    }
}
