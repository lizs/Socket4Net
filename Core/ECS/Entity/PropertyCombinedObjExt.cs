
using System;
using System.Collections.Generic;
using Pi.Core.Protocol;

namespace Pi.Core
{
    /// <summary>
    /// GO属性接口实现
    /// </summary>
    public abstract partial class Go
    {
        public  T Get<T>(EPropertyId id)
        {
            T ret;
            return PropertyBody.Get(id, out ret) ? ret : default(T);
        }

        public  bool Get<T>(EPropertyId id, out T value)
        {
            return PropertyBody.Get(id, out value);
        }

        public  List<T> GetList<T>(EPropertyId id)
        {
            return Get<List<T>>(id);
        }

        public  bool Inject(IBlock block) 
        { 
            return PropertyBody.Inject(block);
        }

        public  bool Inject(EPropertyId pid)
        {
            var block = PropertyInfo.Create(PropertyBody, pid);
            return block != null && PropertyBody.Inject(block);
        }

        public bool Inject(EPropertyId pid, bool serializable)
        {
            var block = PropertyInfo.Create(PropertyBody, pid, serializable);
            return block != null && PropertyBody.Inject(block);
        }

        public  bool MultiInject(params EPropertyId[] pids)
        {
            var result = true;
            foreach (var pid in pids)
            {
                var block = PropertyInfo.Create(PropertyBody, pid);
                result = result & PropertyBody.Inject(block);
            }
            return result;
        }

        private void NotifyPropertyChanged(EPropertyId pid)
        {
            if (Booted)
                NotifyPropertyChanged(PropertyBody.Blocks[pid]);
        }

        #region increasable
        public  bool Inc<T>(EPropertyId id, T delta)
        {
            T overflow;
            return Inc(id, delta, out overflow);
        }
        public  bool Inc(EPropertyId id, object delta)
        {
            object overflow;
            return Inc(id, delta, out overflow);
        }
        public  bool Inc<T>(EPropertyId id, T delta, out T overflow)
        {
            if (!PropertyBody.Inc(id, delta, out overflow)) return false;
            NotifyPropertyChanged(id);
            return true;
        }
        public  bool Inc(EPropertyId id, object delta, out object overflow)
        {
            if (!PropertyBody.Inc(id, delta, out overflow)) return false;
            NotifyPropertyChanged(id);
            return true;
        }

        public  bool IncTo<T>(EPropertyId id, T target)
        {
            if (!PropertyBody.IncTo(id, target)) return false;
            NotifyPropertyChanged(id);
            return true;
        }

        public  bool IncTo(EPropertyId id, object target)
        {
            if (!PropertyBody.IncTo(id, target)) return false;
            NotifyPropertyChanged(id);
            return true;
        }
        #endregion increasable

        #region settable
        public  bool Set<T>(EPropertyId id, T value)
        {
            if (!PropertyBody.Set(id, value)) return false;
            NotifyPropertyChanged(id);
            return true;
        }
        public  bool Set(EPropertyId id, object value)
        {
            if (!PropertyBody.Set(id, value)) return false;
            NotifyPropertyChanged(id);
            return true;
        }
        #endregion settable

        #region list
        public  bool Add<T>(EPropertyId id, T value)
        {
            if (!PropertyBody.Add(id, value)) return false;
            NotifyPropertyChanged(id);
            return true;
        }
        public  bool Add(EPropertyId id, object value)
        {
            if (!PropertyBody.Add(id, value)) return false;
            NotifyPropertyChanged(id);
            return true;
        }
        public  bool AddRange<T>(EPropertyId id, List<T> items)
        {
            if (!PropertyBody.AddRange(id, items)) return false;
            NotifyPropertyChanged(id);
            return true;
        }
        public  bool Remove<T>(EPropertyId id, T item)
        {
            if (!PropertyBody.Remove(id, item)) return false;
            NotifyPropertyChanged(id);
            return true;
        }
        public  bool Remove(EPropertyId id, object item)
        {
            if (!PropertyBody.Remove(id, item)) return false;
            NotifyPropertyChanged(id);
            return true;
        }
        public  bool RemoveAll<T>(EPropertyId id, Predicate<T> predicate, out int count)
        {
            if (!PropertyBody.RemoveAll(id, predicate, out count)) return false;
            NotifyPropertyChanged(id);
            return true;
        }
        public  bool RemoveAll(EPropertyId id, out int count)
        {
            if (!PropertyBody.RemoveAll(id, out count)) return false;
            NotifyPropertyChanged(id);
            return true;
        }

        public  bool Insert<T>(EPropertyId id, int idx, T item)
        {
            if (!PropertyBody.Insert(id, idx, item)) return false;
            NotifyPropertyChanged(id);
            return true;
        }

        public  bool Insert(EPropertyId id, int idx, object item)
        {
            if (!PropertyBody.Insert(id, idx, item)) return false;
            NotifyPropertyChanged(id);
            return true;
        }

        public  bool Replace<T>(EPropertyId id, int idx, T item, out object old)
        {
            if (!PropertyBody.Replace(id, idx, item, out old)) return false;
            NotifyPropertyChanged(id);
            return true;
        }

        public  bool Replace(EPropertyId id, int idx, object item, out object old)
        {
            if (!PropertyBody.Replace(id, idx, item, out old)) return false;
            NotifyPropertyChanged(id);
            return true;
        }
        #endregion list
    }
}
