using System;
using System.Collections.Generic;

namespace socket4net
{
    [Flags]
    public enum EBlockMode
    {
        // 临时的
        Temporary = 0,
        // 需同步
        Synchronizable = 1,
        // 需存储
        Persistable = 2,
    }

    public interface IBlock : ISerializable /*where TKey : IComparable, IEquatable, IComparable*/
    {
        /// <summary>
        ///     属性Id
        /// </summary>
        short Id { get; }

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
        ///     将Value强转成List
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        List<T> AsList<T>();
        
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
    ///     存储旧值的block
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    public interface ITrackableBlock<out TItem> : IBlock
    {
        /// <summary>
        ///     旧值
        /// </summary>
        TItem PreviousValue { get; }
    }

    /// <summary>
    ///     可重新设值的属性块
    /// </summary>
    public interface ISettableBlock<TItem> : ITrackableBlock<TItem>
    {
        void Set(TItem value);
    }

    /// <summary>
    ///     可增减值的属性块
    /// </summary>
    public interface IIncreasableBlock<TItem> : ITrackableBlock<TItem>
    {
        void Inc(TItem delta, out TItem overflow);
        void IncTo(TItem taget);
    }

    public interface IListBlock : IBlock
    {
        int RemoveAll();
    }

    /// <summary>
    ///     列表块
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TItem"></typeparam>
    public interface IListBlock<TItem> : IListBlock, IDifferentialSerializable
    {
        Type ItemType { get; }
        int IndexOf(TItem item);
        int IndexOf(Predicate<TItem> condition);
        void Add(TItem item);
        bool Insert(int idx, TItem item);
        void MultiAdd(IReadOnlyCollection<TItem> items);
        void MultiRemove(IReadOnlyCollection<TItem> items);
        bool Remove(TItem item);
        bool RemoveAt(int idx);
        int RemoveAll(Predicate<TItem> predicate);
        TItem GetByIndex(int idx);
        bool Replace(int idx, TItem item);
        bool Swap(int idxA, int idxB);
    }
}
