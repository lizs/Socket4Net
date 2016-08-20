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
    public class UniqueMgrArg : ObjArg
    {
        public UniqueMgrArg(IObj parent) : base(parent)
        {
        }
    }

    //public enum EContainerOps
    //{
    //    Invalid,
    //    Add,
    //    Remove,
    //}

    public class UniqueMgr<TContainer, TKey, TValue> : Obj, IEnumerable<TValue> 
        where TContainer : IDictionary<TKey, TValue>, new()
        where TValue : class, IUniqueObj<TKey>
    {
        public event Action<TValue> EventObjCreated;
        public event Action<TValue> EventDefaultObjCreated;
        public event Action<TKey, Type> EventObjDestroyed;
        protected readonly TContainer Items = new TContainer();

        public List<TValue> OrderedValues
        {
            get
            {
                var lst = this.ToList();
                lst.Sort((x, y) => x.CompareTo(y));
                return lst;
            }
        }

        public List<TValue> ReverseOrderedValues
        {
            get
            {
                var lst = this.ToList();
                lst.Sort((x, y) => -x.CompareTo(y));
                return lst;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            foreach (var item in ReverseOrderedValues)
                item.Destroy();

            Items.Clear();

            // 清空所有订阅者
            EventObjCreated = null;
            EventDefaultObjCreated = null;
            EventObjDestroyed = null;
        }

        protected override void OnStart()
        {
            base.OnStart();
            foreach (var item in OrderedValues)
                item.Start();
        }

        protected override void OnBorn()
        {
            base.OnBorn();

            foreach (var item in OrderedValues)
                item.Born();
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            return Items.Select(x => x.Value).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #region 创建

        /// <summary>
        ///     创建
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arg"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public T Create<T>(UniqueObjArg<TKey> arg, bool start = false) where T : class, TValue, new()
        {
            if (Exist(arg.Key)) return null;

            var ret = New<T>(arg, start);
            Add(ret);
            EventObjCreated?.Invoke(ret);

            return ret;
        }

        /// <summary>
        ///     非泛型创建
        /// </summary>
        /// <param name="type"></param>
        /// <param name="arg"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public IObj Create(Type type, UniqueObjArg<TKey> arg, bool start = false)
        {
            if (Exist(arg.Key)) return null;
            if (!type.IsSubclassOf(typeof(TValue))) return null;

            var ret = New(type, arg, start);
            var obj = ret as TValue;
            Add(obj);
            EventObjCreated?.Invoke(obj);
            return ret;
        }

        /// <summary>
        ///     非泛型创建，并cast成T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <param name="arg"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public T Create<T>(Type type, UniqueObjArg<TKey> arg, bool start = false) where T : class, TValue
        {
            if (Exist(arg.Key)) return null;
            if (!type.IsSubclassOf(typeof(T))) return null;

            return Create(type, arg, start) as T;
        }

        /// <summary>
        ///     创建默认
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arg"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public T CreateDefault<T>(UniqueObjArg<TKey> arg, bool start) where T : class, TValue, new()
        {
            if (Exist(arg.Key)) return null;

            var ret = Create<T>(arg, start);
            ret.Born();
            Add(ret);
            EventDefaultObjCreated?.Invoke(ret);

            return ret;
        }

        /// <summary>
        ///     非泛型创建默认
        /// </summary>
        /// <param name="type"></param>
        /// <param name="arg"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public IObj CreateDefault(Type type, UniqueObjArg<TKey> arg, bool start)
        {
            if (Exist(arg.Key)) return null;
            if (!type.IsSubclassOf(typeof(TValue))) return null;

            var ret = Create(type, arg, start);
            ret.Born();

            var obj = ret as TValue;
            Add(obj);
            EventDefaultObjCreated?.Invoke(obj);

            return ret;
        }

        /// <summary>
        ///     非泛型创建默认，并cast成T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <param name="arg"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public T CreateDefault<T>(Type type, UniqueObjArg<TKey> arg, bool start) where T : class, TValue
        {
            if (Exist(arg.Key)) return null;
            if (type.IsSubclassOf(typeof(T))) return null;

            return CreateDefault(type, arg, start) as T;
        }
        #endregion

        #region 销毁

        /// <summary>
        ///     destory all items
        /// </summary>
        public void DestroyAll()
        {
            var keys = Items.Keys.ToList();
            keys.ForEach(k => Destroy(k));
        }

        /// <summary>
        ///     destroy items that match "condition"
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public List<TKey> Destroy(Predicate<TValue> condition)
        {
            var victims = Remove(condition);
            var keys = victims.Select(x => x.Id).ToList();
            foreach (var victim in victims)
                victim.Destroy();

            return keys;
        }

        /// <summary>
        ///     destroy all items of type "T"
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<TKey> Destroy<T>() where T : TValue
        {
            var victims = Remove<T>();
            var keys = victims.Select(x => x.Id).ToList();
            foreach (var victim in victims)
                victim.Destroy();

            return keys;
        }

        /// <summary>
        ///     destroy items of type "T" that match "condition"
        /// </summary>
        /// <param name="condition"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<TKey> Destroy<T>(Predicate<T> condition) where T : TValue
        {
            var victims = Remove(condition);
            var keys = victims.Select(x => x.Id).ToList();
            foreach (var victim in victims)
                victim.Destroy();

            return keys;
        }

        /// <summary>
        ///     destroy item specified by "key"
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Destroy(TKey key)
        {
            var obj = Remove(key);
            if (obj == null) return false;

            obj.Destroy();
            EventObjDestroyed?.Invoke(key, obj.GetType());

            return true;
        }

        /// <summary>
        ///     destroy item specified by "value"
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Destroy(TValue value)
        {
            return Destroy(value.Id);
        }

        #endregion

        #region get

        public int Count => Items.Count;

        public TValue this[TKey key] => Get(key);

        public bool Exist(TKey key)
        {
            return Items.ContainsKey(key);
        }

        public bool Exist(Predicate<TValue> condition)
        {
            return Items.Select(x => x.Value).Any(x => condition(x));
        }

        public bool Exist<T>(Predicate<T> condition) where T : TValue
        {
            return Items.Select(x => x.Value).OfType<T>().Any(x => condition(x));
        }

        public TValue Get(TKey key)
        {
            return Items.ContainsKey(key) ? Items[key] : null;
        }

        public TValue GetFirst(Predicate<TValue> condition)
        {
            return this.FirstOrDefault(x => condition(x));
        }

        public List<TValue> Get(Predicate<TValue> condition)
        {
            return this.Where(go => condition(go)).ToList();
        }

        public List<T> Get<T>(Predicate<T> condition) where T : TValue
        {
            return this.OfType<T>().Where(go => condition(go)).ToList();
        }

        public List<T> Get<T>(Predicate<T> condition, int cnt) where T : TValue
        {
            var possiable = Get(condition);
            return possiable.GetRange(0, cnt);
        }

        public T GetFirst<T>(Predicate<T> condition) where T : TValue
        {
            return this.OfType<T>().FirstOrDefault(go => condition(go));
        }

        public T Get<T>(TKey key) where T : TValue
        {
            var ret = Get(key);
            return ret != null ? (T)ret : default(T);
        }

        public List<T> Get<T>() where T : TValue
        {
            return this.OfType<T>().ToList();
        }

        public T GetFirst<T>() where T : TValue
        {
            return Get<T>().FirstOrDefault();
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected bool Add(TValue item)
        {
            if (Items.ContainsKey(item.Id)) return false;
            Items.Add(item.Id, item);

            return true;
        }

        #region remove
        protected TValue Remove(TKey key)
        {
            if (!Items.ContainsKey(key)) return null;

            var item = Items[key];
            Items.Remove(key);

            return item;
        }

        protected List<TValue> Remove(Predicate<TValue> condition)
        {
            var victims = Get(condition);
            victims.ForEach(x => Items.Remove(x.Id));
            return victims;
        }

        protected List<T> Remove<T>() where T : TValue
        {
            var victims = Get<T>();
            victims.ForEach(x => Items.Remove(x.Id));
            return victims;
        }

        protected List<T> Remove<T>(Predicate<T> condition) where T : TValue
        {
            var victims = Get(condition);
            victims.ForEach(x => Items.Remove(x.Id));
            return victims;
        }
        #endregion remove
    }

    /// <summary>
    ///     对象管理器
    ///     对象之间以id唯一区分
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class UniqueMgr<TKey, TValue> : UniqueMgr<Dictionary<TKey, TValue>, TKey, TValue>
        where TValue : class, IUniqueObj<TKey>
    {
    }
}