using System;
using System.Collections.Generic;

namespace socket4net
{
    public interface IData
    {
        #region Êý¾Ý²Ù×÷

        void Inject(IEnumerable<IBlock> blocks);
        List<T> GetList<T>(short pid);
        bool Inject(IBlock block);
        bool Inc<T>(short pid, T delta);
        bool Inc(short pid, object delta);
        bool Inc<T>(short pid, T delta, out T overflow);
        bool Inc(short pid, object delta, out object overflow);
        bool IncTo<T>(short pid, T target);
        bool IncTo(short pid, object target);
        bool Set<T>(short pid, T value);
        bool Set(short pid, object value);
        int IndexOf<T>(short pid, T item);
        int IndexOf<T>(short pid, Predicate<T> condition);
        T GetByIndex<T>(short pid, int idx);
        bool Add<T>(short pid, T value);
        bool Add(short pid, object value);
        bool AddRange<T>(short pid, List<T> items);
        bool Remove<T>(short pid, T item);
        bool Remove(short pid, object item);
        bool RemoveAll<T>(short pid, Predicate<T> predicate);
        bool RemoveAll<T>(short pid, Predicate<T> predicate, out int count);
        bool RemoveAll(short pid);
        bool RemoveAll(short pid, out int count);
        bool Insert<T>(short pid, int idx, T item);
        bool Insert(short pid, int idx, object item);
        bool Replace<T>(short pid, int idx, T item);
        bool Swap<T>(short pid, int idxA, int idxB);

        #endregion
    }
}