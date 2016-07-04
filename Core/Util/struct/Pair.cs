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
using ProtoBuf;

namespace socket4net
{
    [ProtoContract]
    public class Pair<TFirst, TSecond> : IParsableFromString
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

        public void Parse(string str)
        {
            var x = str.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (x.Length != 3)
            {
                throw new IndexOutOfRangeException("[Treble # ParseFromString]:数组大小需要为3");
            }

            Key = x[0].To<TFirst>();
            Value = x[1].To<TSecond>();
        }
    }

    [ProtoContract]
    public class Pair<T> : Pair<T, T>
    {
        public Pair()
        { }
        public Pair(T key, T value)
            : base(key, value)
        {
        }
    }
}
