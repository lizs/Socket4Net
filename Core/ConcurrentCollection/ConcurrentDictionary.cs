using System.Collections.Generic;

namespace Core.ConcurrentCollection
{
    /// <summary>
    /// 并发字典的实现非常简陋，只是为了兼容.net3.5
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class ConcurrentDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private readonly Dictionary<TKey, TValue> _dictionary = new Dictionary<TKey, TValue>(); 
        private readonly object _syncRoot = new object();

        public int Count
        {
            get
            {
                lock (_syncRoot)
                {
                    return _dictionary.Count;
                }
            }
        }

        public bool ContainsKey(TKey key)
        {
            lock (_syncRoot)
            {
                return _dictionary.ContainsKey(key);
            }
        }

        public void Add(TKey key, TValue value)
        {
            lock (_syncRoot)
            {
                Add(key, value);
            }
        }

        public bool Remove(TKey key)
        {
            lock (_syncRoot)
            {
                return _dictionary.Remove(key);
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            lock (_syncRoot)
            {
                return _dictionary.TryGetValue(key, out value);
            }
        }

        public bool TryRemove(TKey key, out TValue value)
        {
            lock (_syncRoot)
            {
                if (_dictionary.ContainsKey(key))
                {
                    value = _dictionary[key];
                    return _dictionary.Remove(key);
                }

                value = default(TValue);
                return false;
            }
        }


        public bool TryAdd(TKey key, TValue value)
        {
            lock (_syncRoot)
            {
                if (_dictionary.ContainsKey(key))
                    return false;

                _dictionary[key] = value;
                return true;
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            lock (_syncRoot)
            {
                foreach (var kv in _dictionary)
                {
                    yield return kv;
                }
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
