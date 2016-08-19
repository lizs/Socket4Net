#region MIT
//  /*The MIT License (MIT)
// 
//  Copyright 2016 lizs lizs4ever@163.com
//  
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//  
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
//  
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
//   * */
#endregion
#if NET35
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace socket4net
{
    /// <summary> Provides blocking and bounding capabilities for thread-safe collections that
    //     implement System.Collections.Concurrent.IProducerConsumerCollection<T>.
    // </summary>
    // <typeparam name="T"></typeparam>
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

        /// <summary>获取 <see cref="T:System.Collections.ICollection" /> 中包含的元素数。</summary>
        /// <returns>
        /// <see cref="T:System.Collections.ICollection" /> 中包含的元素数。</returns>
        /// <filterpriority>2</filterpriority>
        public int Count => _queue.Count;

        private readonly object _syncRoot = new object();

        /// <summary>获取可用于同步 <see cref="T:System.Collections.ICollection" /> 访问的对象。</summary>
        /// <returns>可用于同步对 <see cref="T:System.Collections.ICollection" /> 的访问的对象。</returns>
        /// <filterpriority>2</filterpriority>
        public object SyncRoot => _syncRoot;

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
            if (!_queue.TryAdd(item)) return false;

            _takeEvent.Set();
            return true;
        }

        public bool TryAdd(T item, int millisecondsTimeout)
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

        public bool Take(out T item)
        {
            if (_queue.TryTake(out item))
            {
                _addEvent.Set();
                return true;
            }

            return false;
        }

        public bool TryTake(out T item, int millisecondsTimeout)
        {
            if (_queue.TryTake(out item))
            {
                _addEvent.Set();
                return true;
            }

            if (_takeEvent.WaitOne(millisecondsTimeout))
            {
                if (_queue.TryTake(out item))
                {
                    _addEvent.Set();
                    return true;
                }
            }

            return false;
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
#endif