using System;
using System.Collections;
using System.Collections.Generic;

namespace socket4net
{
    public enum EBlockMode
    {
        // 临时的
        Temporary,
        // 可同步
        Synchronizable,
        // 即时存储
        RealtimePersistable,
    }

    public interface IBlock : ISerializable
    {

        /// <summary>
        ///     当前值
        /// </summary>
        object Value { get; }

        /// <summary>
        ///     值类型
        /// </summary>
        Type Type { get; }

        /// <summary>
        ///     Block分类
        /// </summary>
        EBlockType EBlockType { get; }

        /// <summary>
        ///     是否涂改过
        /// </summary>
        bool Dirty { get; set; }

        /// <summary>
        ///     最近一次涂改的操作
        /// </summary>
        IBlockOps Ops { get; }

        /// <summary>
        ///     Obj是否为T类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        bool Is<T>();

        /// <summary>
        ///     Obj是否为type类型
        /// </summary>
        /// <returns></returns>
        bool Is(Type type);

        /// <summary>
        ///     将Value强转成T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T As<T>();

        /// <summary>
        ///     将Value强转成List<T>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        List<T> AsList<T>();
        
        /// <summary>
        ///     Redis字段名
        /// </summary>
        string RedisFeild { get; }

        /// <summary>
        ///     模式
        /// </summary>
        EBlockMode Mode { get; }

        bool Persistable { get; }
        bool Synchronizable { get; }

        /// <summary>
        ///     设置是否可序列化（需要存储Redis）
        /// </summary>
        /// <param name="mode"></param>
        void SetMode(EBlockMode mode);
    }

    /// <summary>
    ///     属性块
    /// </summary>
    public interface IBlock<TKey> : IBlock
    {      
        /// <summary>
        ///     属性Id
        /// </summary>
        TKey Id { get; }

        /// <summary>
        ///     所在Property
        /// </summary>
        PropertyBody<TKey> Host { get; }
    }

    /// <summary>
    /// 可重新设值的属性块
    /// </summary>
    public interface ISettableBlock<TKey> : IBlock<TKey>
    {
        void Set(object value);
    }

    public interface ISettableBlock<TKey, in TItem> : ITrackableBlock<TKey>
    {
        void Set(TItem value);
    }

    public interface ITrackableBlock<TKey> : IBlock<TKey>
    {
        /// <summary>
        ///     旧值
        /// </summary>
        object PreviousValue { get; }

        /// <summary>
        ///     cast旧值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T PreviousAs<T>();
    }

    /// <summary>
    /// 可增减值的属性块
    /// </summary>
    public interface IIncreasableBlock<TKey> : ITrackableBlock<TKey>
    {
        void Inc(object delta, out object overflow);
        void IncTo(object target);
    }
    public interface IIncreasableBlock<TKey, TItem> : IIncreasableBlock<TKey>
    {
        void Inc(TItem delta, out TItem overflow);
        void IncTo(TItem taget);
    }

    public interface IListBlock<TKey> : IBlock<TKey>, IDifferentialSerializable
    {
        Type ItemType { get; }
        void Add(object item);
        bool Insert(int idx, object item);
        bool Update(int idx);
        bool Remove(object item);
        void MultiAdd(IList items);
        void MultiRemove(List<object> items);
        int RemoveAll();
    }

    public interface IListBlock<TKey, TItem> : IListBlock<TKey>
    {
        int IndexOf(TItem item);
        int IndexOf(Predicate<TItem> condition);
        void Add(TItem item);
        bool Insert(int idx, TItem item);
        void MultiAdd(List<TItem> items);
        void MultiRemove(List<TItem> items);
        bool Remove(TItem item);
        bool RemoveAt(int idx);
        int RemoveAll(Predicate<TItem> predicate);
        TItem GetByIndex(int idx);
        bool Replace(int idx, TItem item);
        bool Swap(int idxA, int idxB);
    }
}
