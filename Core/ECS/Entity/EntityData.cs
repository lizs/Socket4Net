using System;
using System.Collections.Generic;

namespace socket4net
{
    /// <summary>
    ///     E(ECS)
    ///     Êý¾Ý²Ù×÷
    /// </summary>
    public partial class Entity
    {
        private DataComponent _data;

        public void Inject(IEnumerable<IBlock> blocks)
        {
            _data.Inject(blocks);
        }

        public T Get<T>(short id)
        {
            return _data.Get<T>(id);
        }

        public bool Get<T>(short id, out T value)
        {
            return _data.Get<T>(id, out value);
        }

        public List<T> GetList<T>(short id)
        {
            return _data.GetList<T>(id);
        }

        public bool Inject(IBlock block)
        {
            return _data.Inject(block);
        }

        public bool Inc<T>(short id, T delta)
        {
            return _data.Inc(id, delta);
        }

        public bool Inc(short id, object delta)
        {
            return _data.Inc(id, delta);
        }

        public bool Inc<T>(short id, T delta, out T overflow)
        {
            return _data.Inc(id, delta, out overflow);
        }

        public bool Inc(short id, object delta, out object overflow)
        {
            return _data.Inc(id, delta, out overflow);
        }

        public bool IncTo<T>(short id, T target)
        {
            return _data.IncTo(id, target);
        }

        public bool IncTo(short id, object target)
        {
            return _data.IncTo(id, target);
        }

        public bool Set<T>(short id, T value)
        {
            return _data.Set(id, value);
        }

        public bool Set(short id, object value)
        {
            return _data.Set(id, value);
        }

        public int IndexOf<T>(short pid, T item)
        {
            return _data.IndexOf(pid, item);
        }

        public int IndexOf<T>(short pid, Predicate<T> condition)
        {
            return _data.IndexOf(pid, condition);
        }

        public T GetByIndex<T>(short pid, int idx)
        {
            return _data.GetByIndex<T>(pid, idx);
        }

        public bool Add<T>(short id, T value)
        {
            return _data.Add(id, value);
        }

        public bool Add(short id, object value)
        {
            return _data.Add(id, value);
        }

        public bool AddRange<T>(short id, List<T> items)
        {
            return _data.AddRange(id, items);
        }

        public bool Remove<T>(short id, T item)
        {
            return _data.Remove(id, item);
        }

        public bool Remove(short id, object item)
        {
            return _data.Remove(id, item);
        }

        public bool RemoveAll<T>(short id, Predicate<T> predicate)
        {
            return _data.RemoveAll(id, predicate);
        }

        public bool RemoveAll<T>(short id, Predicate<T> predicate, out int count)
        {
            return _data.RemoveAll(id, predicate, out count);
        }

        public bool RemoveAll(short id)
        {
            return _data.RemoveAll(id);
        }

        public bool RemoveAll(short id, out int count)
        {
            return _data.RemoveAll(id, out count);
        }

        public bool Insert<T>(short id, int idx, T item)
        {
            return _data.Insert(id, idx, item);
        }

        public bool Insert(short id, int idx, object item)
        {
            return _data.Insert(id, idx, item);
        }

        public bool Update(short id, int idx)
        {
            return _data.Update(id, idx);
        }

        public bool Replace<T>(short id, int idx, T item)
        {
            return _data.Replace(id, idx, item);
        }

        public bool Swap<T>(short id, int idxA, int idxB)
        {
            return _data.Swap<T>(id, idxA, idxB);
        }
    }
}