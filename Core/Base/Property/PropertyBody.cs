using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace socket4net
{
    public class PropertyBodyArg : ObjArg
    {
        public PropertyBodyArg(IObj owner) : base(owner)
        {
        }
    }

    public class PropertyBody : Obj, IEnumerable<IBlock>
    {
        protected override void OnInit(ObjArg objArg)
        {
            base.OnInit(objArg);
            Blocks = new Dictionary<short, IBlock>();
        }

        public Dictionary<short, IBlock> Blocks { get; private set; }

        public bool Contains(short pid)
        {
            return Blocks.ContainsKey(pid);
        }

        public bool Contains<TItem>(short pid)
        {
            return Blocks.ContainsKey(pid) && Blocks[pid].Is<TItem>();
        }

        public bool Contains(short pid, Type type)
        {
            return Blocks.ContainsKey(pid) && Blocks[pid].Is(type);
        }

        public bool Get<TItem>(short pid, out TItem value)
        {
            value = default(TItem);
            if (!Contains<TItem>(pid)) return false;
            value = Blocks[pid].As<TItem>();
            return true;
        }

        public virtual bool Inc<TItem>(short pid, TItem delta, out TItem overflow)
        {
            overflow = default(TItem);
            if (!Contains<TItem>(pid)) return false;

            var incBlock = Blocks[pid] as IIncreasableBlock<TItem>;
            if(incBlock == null) return false;
            
            incBlock.Inc(delta, out overflow);
            return true;
        }

        public virtual bool Inc(short pid, object delta, out object overflow)
        {
            overflow = null;
            if (!Contains(pid, delta.GetType())) return false;

            var incBlock = Blocks[pid] as IIncreasableBlock<short>;
            if (incBlock == null) return false;

            incBlock.Inc(delta, out overflow);
            return true;
        }

        public bool IncTo(short pid, object target)
        {
            if (!Contains(pid, target.GetType())) return false;

            var incBlock = Blocks[pid] as IIncreasableBlock<short>;
            if(incBlock == null) return false;

            incBlock.IncTo(target);
            return true;
        }

        public bool IncTo<T>(short pid, T target)
        {
            if (!Contains<T>(pid)) return false;

            var incBlock = Blocks[pid] as IIncreasableBlock<T>;
            if(incBlock == null) return false;
            
            incBlock.IncTo(target);
            return true;
        }

        public virtual bool Set(short pid, object value)
        {
            if (value != null && !Contains(pid, value.GetType())) return false;

            var settableBlock = Blocks[pid] as ISettableBlock;
            if (settableBlock == null) return false;
            
            settableBlock.Set(value);
            return true;
        }

        public virtual bool Set<T>(short pid, T value)
        {
            if (!Contains(pid, typeof(T))) return false;

            var settableBlock = Blocks[pid] as ISettableBlock<T>;
            if (settableBlock == null) return false;
            if (Equals<T>.Function(settableBlock.As<T>(), value)) return false;

            settableBlock.Set(value);
            return true;
        }

        #region 列表

        public int IndexOf<TItem>(short pid, Predicate<TItem> condition)
        {
            if (!Contains<List<ListItemRepresentation<TItem>>>(pid)) return Block.InvalidIndex;
            var lst = Blocks[pid] as IListBlock<TItem>;
            if (lst == null) return Block.InvalidIndex;

            return lst.IndexOf(condition);
        }

        public int IndexOf<TItem>(short pid, TItem item)
        {
            if (!Contains<List<ListItemRepresentation<TItem>>>(pid)) return Block.InvalidIndex;
            var lst = Blocks[pid] as IListBlock<TItem>;
            if (lst == null) return Block.InvalidIndex;

            return lst.IndexOf(item);
        }

        public TItem GetByIndex<TItem>(short pid, int idx)
        {
            if (!Contains<List<ListItemRepresentation<TItem>>>(pid)) return default(TItem);
            var lst = Blocks[pid] as IListBlock<TItem>;
            if (lst == null) return default(TItem);

            return lst.GetByIndex(idx);
        }

        public virtual bool Add<TItem>(short pid, TItem item)
        {
            if (!Contains<List<ListItemRepresentation<TItem>>>(pid)) return false;

            var lstBlock = Blocks[pid] as IListBlock<TItem>;
            if (lstBlock == null) return false;

            lstBlock.Add(item);
            return true;
        }

        public virtual bool Add(short pid, object item)
        {
            var lstBlock = Blocks[pid] as IListBlock<short>;
            if (lstBlock == null || lstBlock.ItemType != item.GetType()) return false;

            lstBlock.Add(item);
            return true;
        }

        public virtual bool MultiAdd<TItem>(short pid, List<TItem> items)
        {
            if (!Contains<List<ListItemRepresentation<TItem>>>(pid)) return false;

            var lstBlock = Blocks[pid] as IListBlock<TItem>;
            if (lstBlock == null) return false;

            lstBlock.MultiAdd(items);
            return true;
        }

        public virtual bool MultiAdd(short pid, IList items)
        {
            var lstBlock = Blocks[pid] as IListBlock<short>;
            if (lstBlock == null) return false;

            if (items.GetType() != typeof (List<>).MakeGenericType(new[] {lstBlock.ItemType})) return false;

            lstBlock.MultiAdd(items);
            return true;
        }

        public bool Insert<T>(short pid, int idx, T item)
        {
            var lstBlock = Blocks[pid] as IListBlock<T>;
            return lstBlock != null && lstBlock.Insert(idx, item);
        }

        public bool Insert(short pid, int idx, object item)
        {
            var lstBlock = Blocks[pid] as IListBlock<short>;
            return lstBlock != null && lstBlock.Insert(idx, item);
        }
        
        public bool Update(short pid, int idx)
        {
            var lstBlock = Blocks[pid] as IListBlock<short>;
            return lstBlock != null && lstBlock.Update(idx);
        }

        public bool Swap<TItem>(short pid, int idxA, int idxB)
        {
            var lstBlock = Blocks[pid] as IListBlock<TItem>;
            return lstBlock != null && lstBlock.Swap(idxA, idxB);
        }

        public bool Replace<TItem>(short pid, int idx, TItem item)
        {
            var lstBlock = Blocks[pid] as IListBlock<TItem>;
            return lstBlock != null && lstBlock.Replace(idx, item);
        }

        public virtual bool Remove<TItem>(short pid, TItem item)
        {
            if (!Contains<List<ListItemRepresentation<TItem>>>(pid)) return false;

            var lstBlock = Blocks[pid] as IListBlock<TItem>;
            return lstBlock != null && lstBlock.Remove(item);
        }

        public virtual bool Remove(short pid, object item)
        {
            if (!Contains(pid)) return false;

            var lstBlock = Blocks[pid] as IListBlock<short>;
            if (lstBlock == null || lstBlock.ItemType != item.GetType()) return false;
            return lstBlock.Remove(item);
        }

        public virtual bool MultiRemove<TItem>(short pid, List<TItem> items)
        {
            if (!Contains<List<ListItemRepresentation<TItem>>>(pid)) return false;

            var lstBlock = Blocks[pid] as IListBlock<TItem>;
            if (lstBlock == null) return false;

            lstBlock.MultiRemove(items);
            return true;
        }

        public virtual bool MultiRemove(short pid, List<object> items)
        {
            if (!Contains(pid)) return false;

            var lstBlock = Blocks[pid] as IListBlock<short>;
            if (lstBlock == null) return false;

            lstBlock.MultiRemove(items);
            return true;
        }

        public virtual bool RemoveAll(short pid, out int count)
        {
            count = 0;
            if (!Contains(pid)) return false;

            var lstBlock = Blocks[pid] as IListBlock<short>;
            if (lstBlock == null) return false;

            count = lstBlock.RemoveAll();
            return true;
        }

        public virtual bool RemoveAll<TItem>(short pid, Predicate<TItem> predicate, out int count)
        {
            count = 0;
            if (!Contains<List<ListItemRepresentation<TItem>>>(pid)) return false;

            var lstBlock = Blocks[pid] as IListBlock<TItem>;
            if (lstBlock == null) return false;

            count = lstBlock.RemoveAll(predicate);
            return true;
        }
#endregion

        public bool Inject(IBlock block)
        {
            if (Blocks.ContainsKey(block.Id))
            {
                Logger.Instance.WarnFormat("Block already exist for {0} of {1}", block.Id, OwnerDescription);
                return false;
            }

            Blocks.Add(block.Id, block);
            return true;
        }

        public IBlock GetBlock(short pid)
        {
            var block = Blocks.ContainsKey(pid) ? Blocks[pid] : null;
            if(block == null)
                Logger.Instance.WarnFormat("Block not exist for {0} of {1}", pid, OwnerDescription);

            return block;
        }

        public IEnumerator<IBlock> GetEnumerator()
        {
            return Blocks.Select(x => x.Value).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var block in this)
            {
                sb.AppendLine(block.ToString());
            }

            return sb.ToString();
        }
    }
}
