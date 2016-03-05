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
using System.Collections.Generic;
using System.Linq;

namespace socket4net
{
    public abstract class Block : IBlock
    {
        public const int InvalidIndex = -1;
        private IBlockOps _ops;
        public virtual bool Dirty { get; set; }
        public short Id { get; set; }
        public object Value { get; protected set; }
        public EBlockMode Mode { get; protected set; }
        public Type Type { get; protected set; }

        /// <summary>
        ///     是否需即时存储
        /// </summary>
        public bool Persistable
        {
            get { return (Mode & EBlockMode.Persistable) != 0; }
        }

        /// <summary>
        ///     是否需同步
        /// </summary>
        public bool Synchronizable
        {
            get { return (Mode & EBlockMode.Synchronizable) != 0; }
        }

        public IBlockOps Ops
        {
            get { return _ops; }
            protected set
            {
                _ops = value;
                if (_ops != null)
                    Dirty = true;
            }
        }

        public abstract EBlockType EBlockType { get; }
        public void SetMode(EBlockMode mode)
        {
            Mode = mode;
        }
        
        protected Block(short id, object value, Type type, EBlockMode mode)
        {
            Id = id;
            Value = value;
            Mode = mode;
            Type = type;
        }
        
        public bool Is<TItem>()
        {
            return Is(typeof (TItem));
        }

        public bool Is(Type type)
        {
            return Type == type || type.IsSubclassOf(Type);
        }

        public virtual TItem As<TItem>()
        {
            return (TItem)Value;
        }

        public List<TItem> AsList<TItem>()
        {
            if (!(this is IListBlock<TItem>))
            {
                Logger.Ins.Error("尝试Cast一个非List，Id : {0}", Id);
                return null;
            }

            var lst = (List<ListItemRepresentation<TItem>>) Value;
            return lst.Select(x => x.Item).ToList();
        }

        public override string ToString()
        {
            return string.Format("{0} : {1}", Id, Value ?? "");
        }

        public void Dispose()
        {
        }

        public abstract byte[] Serialize();
        public abstract bool Deserialize(byte[] data);
    }

    public abstract class Block<TItem> : Block
    {
        protected Block(short id, TItem value, EBlockMode mode)
            : base(id, value, typeof(TItem), mode)
        {
        }

        public TItem Get()
        {
            return (TItem)Value;
        }

        protected virtual void InternalSet(TItem value)
        {
            Value = value;
            Dirty = true;
        }

        public override byte[] Serialize()
        {
            return PiSerializer.SerializeValue((TItem)Value);
        }

        public override bool Deserialize(byte[] data)
        {
            if(data.IsNullOrEmpty()) return false;
            Value = PiSerializer.DeserializeValue<TItem>(data);
            return true;
        }
    }
}