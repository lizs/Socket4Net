using System;
using System.Collections.Generic;
using System.Linq;

namespace socket4net
{
#if NET35
    /// <summary>
    ///     stack
    ///     implemented by List
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Stack<T>
    {
        private readonly List<T> _inner;

        public Stack()
        {
            _inner = new List<T>();
        }

        public Stack(IEnumerable<T> collection)
        {
            _inner = new List<T>(collection);
        }

        public Stack(int capacity)
        {
            _inner = new List<T>(capacity);
        }
        public int Count { get { return _inner.Count; } }

        public void Clear()
        {
             _inner.Clear();
        }

        public bool Contains(T item)
        {
            return  _inner.Contains(item);
        }

        public T Peek()
        {
            return _inner.LastOrDefault();
        }

        public T Pop()
        {
            if(_inner.IsNullOrEmpty())
                throw new Exception("Stack is empty!");

            var ret = _inner.Last();
            _inner.RemoveAt(_inner.Count - 1);
            return ret;
        }

        public void Push(T item)
        {
            _inner.Add(item);
        }
    }
#endif
}
