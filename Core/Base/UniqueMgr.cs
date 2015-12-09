using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace socket4net
{
    public class UniqueMgrArg : RootedObjArg
    {
        public UniqueMgrArg(IObj parent) : base(parent)
        {
        }
    }

    public enum EContainerOps
    {
        Invalid,
        Add,
        Remove,
    }

    /// <summary>
    ///     对象管理器
    ///     对象之间以id唯一区分
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class UniqueMgr<TKey, TValue> : RootedObj, IEnumerable<TValue> where TValue : class ,IUniqueObj<TKey>
    {
        public event Action<TValue, EContainerOps> EventContainerChanged;
        protected readonly Dictionary<TKey, TValue> Items = new Dictionary<TKey, TValue>();

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

        protected override void OnInit()
        {
            base.OnInit();
            foreach (var item in OrderedValues)
                item.Init();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            foreach (var item in ReverseOrderedValues)
                item.Destroy();

            Items.Clear();

            // 清空所有订阅者
            EventContainerChanged = null;
        }

        protected override void OnStart()
        {
            base.OnStart();
            foreach (var item in OrderedValues)
                item.Start();
        }

        public override void AfterStart()
        {
            base.AfterStart();
            foreach (var item in OrderedValues)
                item.AfterStart();
        }

        protected override void OnReset()
        {
            base.OnReset();

            foreach (var item in OrderedValues)
                item.Reset();
        }
        
        public IEnumerator<TValue> GetEnumerator()
        {
            return Items.Select(x => x.Value).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        ///     创建
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arg"></param>
        /// <param name="init"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public T Create<T>(UniqueObjArg<TKey> arg, bool init, bool start, bool notify = true) where T : TValue, new()
        {
            if (Exist(arg.Key)) return default(T);

            var ret = ObjFactory.Create<T>(arg);
            if (start)
            {
                // 需要Start的对象必须初始先
                ret.Init();
                ret.Start();
                ret.AfterStart();
            }
            else if (init)
            {
                ret.Init();
            }

            Add(ret, notify);

            return ret;
        }

        #region get

        public int Count
        {
            get { return Items.Count; }
        }

        public TValue this[TKey key]
        {
            get { return Get(key); }
        }

        public bool Exist(TKey key)
        {
            return Items.ContainsKey(key);
        }

        public bool Exist(Predicate<TValue> condition)
        {
            return Items.Select(x => x.Value).Any(x=>condition(x));
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
            return this.FirstOrDefault(x=>condition(x));
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
      
        #region destroy

        public void DestroyAll(bool notify = true)
        {
            var keys = Items.Keys.ToList();
            keys.ForEach(k=>Destroy(k, notify));
        }

        public List<TKey> Destroy(Predicate<TValue> condition, bool notify = true)
        {
            var victims = Remove(condition, notify);
            var keys = victims.Select(x => x.Id).ToList();
            foreach (var victim in victims)
                victim.Destroy();

            return keys;
        } 

        public List<TKey> Destroy<T>(bool notify = true) where T : TValue
        {
            var victims = Remove<T>(notify);
            var keys = victims.Select(x => x.Id).ToList();
            foreach (var victim in victims)
                victim.Destroy();

            return keys;
        }

        public List<TKey> Destroy<T>(Predicate<T> condition, bool notify = true) where T : TValue
        {
            var victims = Remove<T>(condition, notify);
            var keys = victims.Select(x => x.Id).ToList();
            foreach (var victim in victims)
                victim.Destroy();

            return keys;
        }

        public bool Destroy(TKey key, bool notify = true)
        {
            var obj = Remove(key, notify);
            if (obj != null)
                obj.Destroy();

            return obj != null;
        }

        public bool Destroy(TValue value, bool notify = true)
        {
            return Destroy(value.Id, notify);
        }
        #endregion

        protected bool Add(TValue item, bool notify = true)
        {
            if (Items.ContainsKey(item.Id)) return false;
            Items.Add(item.Id, item);

            if (notify && EventContainerChanged != null)
                EventContainerChanged(item, EContainerOps.Add);

            return true;
        }

        #region remove
        protected TValue Remove(TKey key, bool notify = true)
        {
            if (!Items.ContainsKey(key)) return null;

            var item = Items[key];
            Items.Remove(key);

            if (notify && EventContainerChanged != null)
                EventContainerChanged(item, EContainerOps.Remove);

            return item;
        }

        protected List<TValue> Remove(Predicate<TValue> condition, bool notify = true)
        {
            var victims = Get(condition);
            victims.ForEach(x =>
            {
                Items.Remove(x.Id);

                if (notify && EventContainerChanged != null)
                    EventContainerChanged(x, EContainerOps.Remove);
            });
            return victims;
        } 

        protected List<T> Remove<T>(bool notify = true) where T : TValue
        {
            var victims = Get<T>();
            victims.ForEach(x =>
            {
                Items.Remove(x.Id);

                if (notify && EventContainerChanged != null)
                    EventContainerChanged(x, EContainerOps.Remove);
            });
            return victims;
        }

        protected List<T> Remove<T>(Predicate<T> condition, bool notify = true) where T : TValue
        {
            var victims = Get<T>(condition);
            victims.ForEach(x =>
            {
                Items.Remove(x.Id);

                if (notify && EventContainerChanged != null)
                    EventContainerChanged(x, EContainerOps.Remove);
            });
            return victims;
        }
        #endregion remove
    }
}