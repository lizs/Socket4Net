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

namespace socket4net
{
    public interface IProducerConsumerCollection<T> : IEnumerable<T>, ICollection
    {
        // Summary:
        //     Copies the elements of the System.Collections.Concurrent.IProducerConsumerCollection<T>
        //     to an System.Array, starting at a specified index.
        //
        // Parameters:
        //   array:
        //     The one-dimensional System.Array that is the destination of the elements
        //     copied from the System.Collections.Concurrent.IProducerConsumerCollection<T>.
        //     The array must have zero-based indexing.
        //
        //   index:
        //     The zero-based index in array at which copying begins.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     array is a null reference (Nothing in Visual Basic).
        //
        //   System.ArgumentOutOfRangeException:
        //     index is less than zero.
        //
        //   System.ArgumentException:
        //     index is equal to or greater than the length of the array -or- The number
        //     of elements in the collection is greater than the available space from index
        //     to the end of the destination array.
        void CopyTo(T[] array, int index);
        //
        // Summary:
        //     Copies the elements contained in the System.Collections.Concurrent.IProducerConsumerCollection<T>
        //     to a new array.
        //
        // Returns:
        //     A new array containing the elements copied from the System.Collections.Concurrent.IProducerConsumerCollection<T>.
        T[] ToArray();
        //
        // Summary:
        //     Attempts to add an object to the System.Collections.Concurrent.IProducerConsumerCollection<T>.
        //
        // Parameters:
        //   item:
        //     The object to add to the System.Collections.Concurrent.IProducerConsumerCollection<T>.
        //
        // Returns:
        //     true if the object was added successfully; otherwise, false.
        //
        // Exceptions:
        //   System.ArgumentException:
        //     The item was invalid for this collection.
        bool TryAdd(T item);
        //
        // Summary:
        //     Attempts to remove and return an object from the System.Collections.Concurrent.IProducerConsumerCollection<T>.
        //
        // Parameters:
        //   item:
        //     When this method returns, if the object was removed and returned successfully,
        //     item contains the removed object. If no object was available to be removed,
        //     the value is unspecified.
        //
        // Returns:
        //     true if an object was removed and returned successfully; otherwise, false.
        bool TryTake(out T item);
    }

    public class ConcurrentQueue<T> : IProducerConsumerCollection<T>, IDisposable
    {
        public int Count { get; private set; }

        private readonly object _syncRoot = new object();
        public object SyncRoot { get { return _syncRoot; } }

        private readonly Queue<T> _queue = new Queue<T>();

        public int Capcity { get; private set; }
        public ConcurrentQueue(int capacity = int.MaxValue)
        {
            Capcity = capacity;
        }

        public bool TryAdd(T item)
        {
            lock (_syncRoot)
            {
                if (_queue.Count < Capcity)
                {
                    _queue.Enqueue(item);
                    return true;
                }

                return false;
            }
        }

        public bool TryTake(out T item)
        {
            lock (_syncRoot)
            {
                item = default(T);
                if (_queue.Count > 0)
                {
                    item = _queue.Dequeue();
                    return true;
                }

                return false;
            }
        }

        public bool TryDequeue(out T item)
        {
            return TryTake(out item);
        }

        public bool Enqueue(T item)
        {
            return TryAdd(item);
        }

        public void Dispose()
        {
            lock (_syncRoot)
            {
                _queue.Clear();
            }
        }


        #region 未实现的接口，暂未用到
        public bool IsSynchronized { get { throw new NotImplementedException(); } }
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

        public void CopyTo(T[] array, int index)
        {
            throw new NotImplementedException();
        }

        public T[] ToArray()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
#endif