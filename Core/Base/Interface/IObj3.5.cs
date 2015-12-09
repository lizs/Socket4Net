using System;
using System.Collections.Generic;
using Pi.Common.Util;
using Pi.Protocol;

namespace Pi.Core
{
    /// <summary>
    /// 工厂批量生产接口
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public interface ICreatable<out TKey> : IIdentityReadonly<TKey>
    {
        void SetParam(object[] param);
    }

    /// <summary>
    /// 对象
    /// 组合了属性，可批量生产
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public interface IObj<out TKey> : IPropertyCombined, IBootable, ICreatable<TKey>
    {
        IObj Owner { get; }
        DateTime LastCommitTime { get; set; }
        IEnumerable<Tuple<string, IBlock>> GetChangedBlocks();
    }

    /// <summary>
    /// long特化
    /// </summary>
    public interface IObj : IObj<long>
    {
    }

    /// <summary>
    /// 组件化接口
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public interface IComponented : IEnumerable<IComponent>
    {
        IMgr<string, IComponent> Components { get; }
    }

    /// <summary>
    /// 游戏对象
    /// </summary>
    public interface IGameObject : IObj, IComponented, IConfigured
    {
        /// <summary>
        /// 全名
        /// </summary>
        string Name { get; }
        /// <summary>
        /// 昵称
        /// </summary>
        string Nick { get; }
        /// <summary>
        /// 类型
        /// </summary>
        PropertyHostType Type { get; }
    }
}
