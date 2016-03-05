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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace socket4net
{
    /// <summary>
    ///     对象管理器
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public class Mgr<TValue> : Obj, IEnumerable<TValue> where TValue : Obj
    {
        private readonly List<TValue> _items = new List<TValue>();

        protected override void OnDestroy()
        {
            base.OnDestroy();
            foreach (var item in this.Reverse())
                item.Destroy();

            _items.Clear();
        }

        protected override void OnStart()
        {
            base.OnStart();
            foreach (var item in this)
                item.Start();
        }

        protected override void OnReset()
        {
            base.OnReset();
            foreach (var item in this)
                item.Reset();
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        public void Destroy<T>() where T : TValue
        {
            var victims = Get<T>();
            _items.RemoveAll(value => value is T);
            foreach (var obj in victims)
                obj.Destroy();
        }

        public void Destroy<T>(Predicate<T> condition) where T : TValue
        {
            var victims = Get<T>(condition);
            _items.RemoveAll(x => x is T && condition(x as T));
            foreach (var obj in victims)
                obj.Destroy();
        }

        public List<T> Get<T>() where T : TValue
        {
            return this.OfType<T>().ToList(); 
        }

        public List<T> Get<T>(Predicate<T> condition) where T : TValue
        {
            return this.OfType<T>().Where(x=>condition(x)).ToList();
        }

        public List<T> GetReverse<T>() where T : TValue
        {
            return this.OfType<T>().Reverse().ToList();
        }

        public T GetFirst<T>() where T : TValue
        {
            return Get<T>().FirstOrDefault();
        }

        public T Create<T>(ObjArg arg, bool start) where T : class, TValue, new()
        {
            var ret = New<T>(arg, start);
            _items.Add(ret);
            return ret;
        }
    }
}