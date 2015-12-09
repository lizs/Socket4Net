using Pi.Protocol;

namespace Pi.Core
{
    public interface IItem : IObj, IConfigured
    {
        GoodsType Type { get; }
        int Level { get; }
        int Count { get; }
        int ConfigId { get; }
    }

    public interface ISmartItem : IItem, ISmartObj { }
}