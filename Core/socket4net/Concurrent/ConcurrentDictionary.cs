#if NET35
using System.Collections;
using System.Collections.Generic;

namespace socket4net
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

        public TValue this[TKey key]
        {
            get
            {
                TValue value;
                return TryGetValue(key, out value) ? value : default(TValue);
            }
        }

        public void Clear()
        {
            lock (_syncRoot)
            {
                _dictionary.Clear();
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
                _dictionary.Add(key, value);
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

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
#endif