using System;
using System.Collections.Generic;

namespace socket4net
{
    public interface IData
    {
        #region Êý¾Ý²Ù×÷

        void Inject(IEnumerable<IBlock> blocks);
        List<T> GetList<T>(short id);
        bool Inject(IBlock block);
        bool Inc<T>(short id, T delta);
        bool Inc(short id, object delta);
        bool Inc<T>(short id, T delta, out T overflow);
        bool Inc(short id, object delta, out object overflow);
        bool IncTo<T>(short id, T target);
        bool IncTo(short id, object target);
        bool Set<T>(short id, T value);
        bool Set(short id, object value);
        int IndexOf<T>(short pid, T item);
        int IndexOf<T>(short pid, Predicate<T> condition);
        T GetByIndex<T>(short pid, int idx);
        bool Add<T>(short id, T value);
        bool Add(short id, object value);
        bool AddRange<T>(short id, List<T> items);
        bool Remove<T>(short id, T item);
        bool Remove(short id, object item);
        bool RemoveAll<T>(short id, Predicate<T> predicate);
        bool RemoveAll<T>(short id, Predicate<T> predicate, out int count);
        bool RemoveAll(short id);
        bool RemoveAll(short id, out int count);
        bool Insert<T>(short id, int idx, T item);
        bool Insert(short id, int idx, object item);
        bool Update(short id, int idx);
        bool Replace<T>(short id, int idx, T item);
        bool Swap<T>(short id, int idxA, int idxB);

        #endregion
    }
}