
namespace Pi.Core
{
    public interface IUnityObj<TKey, out TValue> where TValue : IObj<TKey>
    {
        TValue Body { get; }
    }
}
