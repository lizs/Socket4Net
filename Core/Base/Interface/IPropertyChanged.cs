using System.Collections.Generic;
using Pi.Protocol;

namespace Pi.Core
{
    /// <summary>
    /// 属性改变通知器
    /// </summary>
    public interface IPropertyNotifier
    {
        string PType { get; }
        void NotifyPropertyChanged(PropertyId id, IBlock value);
    }

    /// <summary>
    /// 过滤属性改变
    /// </summary>
    public interface IPropertyObserver
    {
        List<PropertyId> PropertyFilter { get; set; }
        bool Filter(PropertyId id);
        void OnPropertyChanged(IPropertyNotifier notifier, PropertyId id, IBlock value);
    }
}