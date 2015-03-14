
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Core.ConcurrentCollection
{
    // Summary:
    //     Provides blocking and bounding capabilities for thread-safe collections that
    //     implement System.Collections.Concurrent.IProducerConsumerCollection<T>.
    //
    // Type parameters:
    //   T:
    //     The type of elements in the collection.
    public class BlockingCollection<T> : IEnumerable<T>, ICollection, IDisposable
    {
        #region Not implemented
        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }
        public bool IsSynchronized { get; private set; }
        #endregion

        public int Count
        {
            get { return _queue.Count; }
        }

        private readonly object _syncRoot = new object();
        public object SyncRoot { get { return _syncRoot; } }

        private ConcurrentQueue<T> _queue;

        public int Capacity { get; private set; }
        public BlockingCollection(int capacity = int.MaxValue)
        {
            Capacity = capacity;
            _queue = new ConcurrentQueue<T>(capacity);
        }

        private readonly AutoResetEvent _addEvent = new AutoResetEvent(false);
        private readonly AutoResetEvent _takeEvent = new AutoResetEvent(false);

        public bool Add(T item)
        {
            lock (_syncRoot)
            {
                if (!_queue.TryAdd(item)) return false;

                _takeEvent.Set();
                return true;
            }
        }

        public bool TryAdd(T item, int millisecondsTimeout)
        {
            lock (_syncRoot)
            {
                if (_queue.TryAdd(item))
                {
                    _takeEvent.Set();
                    return true;
                }

                if (_addEvent.WaitOne(millisecondsTimeout))
                {
                    if (_queue.TryAdd(item))
                    {
                        _takeEvent.Set();
                        return true;
                    }
                }

                return false;
            }
        }

        public bool Take(out T item)
        {
            lock (_syncRoot)
            {
                if (_queue.TryTake(out item))
                {
                    _addEvent.Set();
                    return true;
                }

                return false;
            }
        }

        public bool TryTake(out T item, int millisecondsTimeout)
        {
            lock (_syncRoot)
            {
                if (_queue.TryTake(out item)) return true;

                if (_takeEvent.WaitOne(millisecondsTimeout))
                {
                    return _queue.TryTake(out item);
                }

                return false;
            }
        }

        public void Dispose()
        {
            lock (_syncRoot)
            {
                if (_queue != null)
                {
                    _queue.Dispose();
                    _queue = null;
                }
            }
        }
    }
}
