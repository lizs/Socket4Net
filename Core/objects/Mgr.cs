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
    public class Mgr<TValue> : Obj, IEnumerable<TValue> where TValue : class, IObj
    {
        private readonly List<TValue> _items = new List<TValue>();

        /// <summary>
        ///    internal called when an Obj is to be destroyed
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();
            foreach (var item in this.Reverse())
                item.Destroy();

            _items.Clear();
        }

        /// <summary>
        ///     Invoked when obj started
        /// </summary>
        protected override void OnStart()
        {
            base.OnStart();
            foreach (var item in this)
                item.Start();
        }

        /// <summary>
        ///     Invoked when obj born
        /// </summary>
        protected override void OnBorn()
        {
            base.OnBorn();
            foreach (var item in this)
                item.Born();
        }

        /// <summary>返回一个循环访问集合的枚举数。</summary>
        /// <returns>可用于循环访问集合的 <see cref="T:System.Collections.Generic.IEnumerator`1" />。</returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<TValue> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        /// <summary>
        ///  destroy all the elements of type "T"
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void Destroy<T>() where T : TValue
        {
            var victims = Get<T>();
            _items.RemoveAll(value => value is T);
            foreach (var obj in victims)
                obj.Destroy();
        }

        /// <summary>
        ///  destroy all the elements of type "T" that satisfied the "condition"
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void Destroy<T>(Predicate<T> condition) where T : class, TValue
        {
            var victims = Get(condition);
            _items.RemoveAll(x => x is T && condition((T) x));
            foreach (var obj in victims)
                obj.Destroy();
        }

        /// <summary>
        ///  get all the elements of type "T"
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<T> Get<T>() where T : TValue
        {
            return this.OfType<T>().ToList(); 
        }

        /// <summary>
        ///  get all the elements of type "T" that satisfied the "condition"
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="condition"></param>
        /// <returns></returns>
        public List<T> Get<T>(Predicate<T> condition) where T : TValue
        {
            return this.OfType<T>().Where(x=>condition(x)).ToList();
        }

        /// <summary>
        ///  get all the elements of type "T" as reverse order
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<T> GetReverse<T>() where T : TValue
        {
            return this.OfType<T>().Reverse().ToList();
        }

        /// <summary>
        ///  get the first element of type "T"
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetFirst<T>() where T : TValue
        {
            return Get<T>().FirstOrDefault();
        }

        /// <summary>
        ///     create object of type "T"
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arg"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public T Create<T>(ObjArg arg, bool start) where T : class, TValue, new()
        {
            var ret = New<T>(arg, start);
            _items.Add(ret);
            return ret;
        }
    }
}