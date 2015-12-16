using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace socket4net
{
    public class ListBlock<TKey, TItem> : Block<TKey, List<ListItemRepresentation<TItem>>>,
        IListBlock<TKey, TItem>
    {
        private int _uidSeed;

        private int GetUid()
        {
            return ++_uidSeed;
        }

        public ListBlock(TKey id, List<TItem> value, EBlockMode mode)
            : base(id, null, mode)
        {
            Value = new List<ListItemRepresentation<TItem>>();

            if (value != null && value.Count > 0)
            {
                MultiAdd(value);
            }
        }

        public override string ToString()
        {
            var lst = Get().Select(x=>x.Item).ToList();
            var sb = new StringBuilder();
            sb.AppendLine(Id.ToString());
            lst.ForEach(x =>
            {
                var tmp = (object) x;
                sb.AppendLine(tmp == null ? "null" : x.ToString());
            });

            var info = sb.ToString();
            if (info.Length == 0)
                info = "空列表";
            return info;
        }

        /// <summary>
        ///     操作栈，有序记录Push之前的所有列表操作
        /// </summary>
        private readonly List<ListOpsRepresentation> _opsStack =
            new List<ListOpsRepresentation>();

        private bool _dirty;
        public override bool Dirty
        {
            get { return _dirty; }
            set
            {
                if (_dirty == value) return;
                _dirty = value;
                if (!_dirty)
                    _opsStack.Clear();
            }
        }

        public Type ItemType
        {
            get { return typeof (TItem); }
        }

        public override EBlockType EBlockType
        {
            get { return EBlockType.List; }
        }

        private void Record(ListOpsRepresentation ops)
        {
            Ops = Ops;
            Dirty = true;
#if NET45
            var ignore = false;
            switch (ops.Ops)
            {
                case EPropertyOps.Remove:
                {
                    // 干掉冗余操作同步
                    var cnt = _opsStack.RemoveAll(x => x.Id == ops.Id);
                    ignore = cnt > 0;
                    break;
                }
            }

            if(!ignore)
                _opsStack.Add(ops);
#else
            // 客户端不记录操作
#endif
        }

        public int IndexOf(Predicate<TItem> condition)
        {
            var lst = Get();
            return lst.FindIndex(x => condition(x.Item));
        }

        public int IndexOf(TItem item)
        {
            var lst = Get();
            return lst.FindIndex(x => Equals(x.Item, item));
        }

        private int GetIndexById(int id)
        {
            var lst = Get();
            var item = lst.Find(x => x.Id == id);
            if (item == null) return InvalidIndex;
            return lst.IndexOf(item);
        }

        public void Add(object item)
        {
            Add((TItem) item);
        }

        public void Add(TItem item)
        {
            Insert(Get().Count, item);
        }

        public bool Remove(object item)
        {
            return Remove((TItem) item);
        }

        public bool Remove(TItem item)
        {
            var idx = IndexOf(item);
            return RemoveAt(idx);
        }

        public bool RemoveAt(int idx)
        {
            var lst = Get();
            if (idx < 0 || idx >= lst.Count) return false;

            var removed = lst[idx];
            lst.RemoveAt(idx);

            Record(new ListRemoveOps(removed.Id));
            return true;
        }

        public bool Insert(int idx, object item)
        {
            return Insert(idx, (TItem) item);
        }

        public bool Insert(int idx, TItem item)
        {
            var lst = Get();
            if (idx < 0 || idx > lst.Count) return false;

            var rep = new ListItemRepresentation<TItem> { Id = GetUid(), Item = item };
            var ops = new ListInsertOps(rep.Id, idx);
            lst.Insert(idx, rep);

            Record(ops);
            return true;
        }

#if NET35
        public bool Insert(int idx, int id, TItem item)
        {
            var lst = Get();
            if (idx < 0 || idx > lst.Count) return false;

            var rep = new ListItemRepresentation<TItem> { Id = id, Item = item };
            var ops = new ListInsertOps(rep.Id, idx);
            lst.Insert(idx, rep);

            Record(ops);
            return true;
        }
#endif

        public bool Replace(int idx, TItem item)
        {
            var lst = Get();
            if (idx < 0 || idx >= lst.Count) return false;

            var updated = lst[idx];
            updated.Item = item;

            Record(new ListUpdateOps(updated.Id));
            return true;
        }

        public bool Swap(int idxA, int idxB)
        {
            var lst = Get();
            if (idxA < 0 || idxA >= lst.Count || idxB < 0 || idxB >= lst.Count) return false;

            var updatedA = lst[idxA];
            var updatedB = lst[idxB];

            var item = updatedA.Item;
            updatedA.Item = updatedB.Item;
            updatedB.Item = item;

            Record(new ListUpdateOps(updatedA.Id));
            Record(new ListUpdateOps(updatedB.Id));
            return true;
        }

        public bool Update(int idx)
        {
            var lst = Get();
            if (idx < 0 || idx >= lst.Count) return false;

            var updated = lst[idx];
            Record(new ListUpdateOps(updated.Id));

            return true;
        }

        public void MultiAdd(IList items)
        {
            MultiAdd((List<TItem>) items);
        }

        public void MultiAdd(List<TItem> items)
        {
            if (items == null || items.Count == 0) return;
            foreach (var item in items)
            {
                Add(item);
            }
        }

        public void MultiRemove(List<object> items)
        {
            MultiRemove(items.Cast<TItem>().ToList());
        }

        public void MultiRemove(List<TItem> items)
        {
            if (items == null) return;

            foreach (var idx in items.Select(IndexOf).Where(idx => idx != -1))
            {
                RemoveAt(idx);
            }
        }

        public int RemoveAll(Predicate<TItem> predicate)
        {
            var toBeRemoved = Get().FindAll(x => predicate(x.Item)).Select(x => x.Item).ToList();
            if (toBeRemoved.Count == 0) return 0;

            MultiRemove(toBeRemoved);
            return toBeRemoved.Count;
        }

        public TItem GetByIndex(int idx)
        {
            var lst = Get();
            if (idx >= 0 && idx < lst.Count)
            {
                return lst[idx].Item;
            }
            return default(TItem);
        }

        public int RemoveAll()
        {
            var lst = Get();
            var cnt = lst.Count;

            while (lst.Count > 0)
                RemoveAt(0);

            return cnt;
        }

        public override byte[] Serialize()
        {
            var lst = Get().Select(x => x.Item).ToList();
            return PiSerializer.SerializeValue(lst);
        }

        public override void Deserialize(byte[] bytes)
        {
            if (bytes.IsNullOrEmpty()) return;

            Get().Clear();
            var lst = PiSerializer.DeserializeValue<List<TItem>>(bytes);
            if (lst == null || lst.Count <= 0) return;

            MultiAdd(lst);
        }

        /// <summary>
        ///     列表差分序列化
        /// </summary>zx
        /// <returns></returns>
        public byte[] SerializeDifference()
        {
            if (_opsStack.Count == 0) return null;
            var proto = new ListOpsProto<TItem> { OpsStack = new List<KeyValuePair<EPropertyOps, byte[]>>() };
            _opsStack.ForEach(
                ops =>
                {
                    switch (ops.Ops)
                    {
                        case EPropertyOps.Insert:
                            {
                                var insert = ops as ListInsertOps;
                                var item = Get().Find(x => x.Id == insert.Id);
                                if (item != null)
                                {
                                    proto.OpsStack.Add(new KeyValuePair<EPropertyOps, byte[]>(EPropertyOps.Insert,
                                        (new InsertOpsProto<TItem> { Index = insert.Index, ListItem = item }).Serialize()));
                                }
                                break;
                            }

                        case EPropertyOps.Remove:
                            {
                                var rm = ops as ListRemoveOps;
                                proto.OpsStack.Add(new KeyValuePair<EPropertyOps, byte[]>(EPropertyOps.Remove,
                                    (new RemoveOpsProto { Id = rm.Id }).Serialize()));
                                break;
                            }

                        case EPropertyOps.Update:
                            {
                                var update = ops as ListUpdateOps;
                                var item = Get().Find(x => x.Id == update.Id);
                                proto.OpsStack.Add(new KeyValuePair<EPropertyOps, byte[]>(EPropertyOps.Update,
                                    (new UpdateOpsProto<TItem> { ListItem = item }).Serialize()));
                                break;
                            }
                    }
                });

            return PiSerializer.Serialize(proto);
        }

        /// <summary>
        ///     列表差分反序列化
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public bool DeserializeDifference(byte[] bytes)
        {
            if (bytes.IsNullOrEmpty()) return true;
            var proto = PiSerializer.Deserialize<ListOpsProto<TItem>>(bytes);
            if (proto.OpsStack == null || proto.OpsStack.Count == 0) return true;

            proto.OpsStack.ForEach(o =>
            {
                switch (o.Key)
                {
                    case EPropertyOps.Insert:
                        {
                            var x = InsertOpsProto<TItem>.Deserialize(o.Value);
                            if (x == null || x.ListItem == null)
                                Logger.Instance.ErrorFormat("插入的item为空，位置：{0}，RedisFeild：{1}", x.Index, RedisFeild);
                            else
                            {
#if NET35
                                Insert(x.Index, x.ListItem.Id, x.ListItem.Item);
#else
                                Insert(x.Index, x.ListItem.Item);
#endif
                            }
                            break;
                        }

                    case EPropertyOps.Remove:
                        {
                            var x = RemoveOpsProto.Deserialize(o.Value);
                            if (x == null)
                                Logger.Instance.ErrorFormat("移除item失败，属性：{0}:{1}", Id, RedisFeild);
                            else
                            {
                                var idx = GetIndexById(x.Id);
                                if (idx == InvalidIndex || !RemoveAt(idx))
                                    Logger.Instance.ErrorFormat("移除item失败，未找到动态id为 {0} 的item", x.Id);
                            }
                            break;
                        }

                    case EPropertyOps.Update:
                        {
                            var x = UpdateOpsProto<TItem>.Deserialize(o.Value);
                            if(x == null || x.ListItem == null)
                                Logger.Instance.Warn("更新数据为空");
                            else
                            {
                                var updated = Get().Find(y => y.Id == x.ListItem.Id);
                                if (updated != null)
                                {
                                    if (updated.Item is ICloneable<TItem>)
                                        (updated.Item as ICloneable<TItem>).CloneFrom(x.ListItem.Item);
                                    else
                                        updated.Item = x.ListItem.Item;
                                }
                                else
                                    Logger.Instance.ErrorFormat("更新的item不存在，动态id : {0}", x.ListItem.Id);
                            }
                          
                            break;
                        }
                }
            });

            return true;
        }
    }
}