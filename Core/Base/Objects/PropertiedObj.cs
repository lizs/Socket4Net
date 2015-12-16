using System;
using System.Collections.Generic;
using System.Linq;
using socket4net.Util;

namespace socket4net
{
    public class PropertiedObjArg<TKey> : UniqueObjArg<long>
    {
        public IEnumerable<Pair<TKey, IBlock<TKey>>> Properties { get; private set; }

        public PropertiedObjArg(IObj parent, long key, IEnumerable<Pair<TKey, IBlock<TKey>>> properties)
            : base(parent, key)
        {
            Properties = properties;
        }
    }

    public interface IPropertiedObj<TPKey> : IUniqueObj<long>
    {
        IBlock<TPKey> GetBlock(TPKey key);
        IEnumerable<string> GetFeilds();
        IEnumerable<IBlock<TPKey>> Blocks { get; }

        #region 属性订阅

        void Listen(Action<IPropertiedObj<TPKey>, IBlock<TPKey>> handler, params TPKey[] pids);
        void Unlisten(Action<IPropertiedObj<TPKey>, IBlock<TPKey>> handler, params TPKey[] pids);

        #endregion

        #region 属性操作

        T Get<T>(TPKey id);
        bool Get<T>(TPKey id, out T value);
        List<T> GetList<T>(TPKey id);
        bool Inject(IBlock<TPKey> block);
        //bool Inject(TPKey pid);
        //bool Inject(TPKey pid, EBlockMode mode);
        //bool MultiInject(params TPKey[] pids);
        void NotifyPropertyChanged(TPKey pid);
        void NotifyPropertyChanged();

        #region increasable

        bool Inc<T>(TPKey id, T delta);
        bool Inc(TPKey id, object delta);
        bool Inc<T>(TPKey id, T delta, out T overflow);
        bool Inc(TPKey id, object delta, out object overflow);
        bool IncTo<T>(TPKey id, T target);
        bool IncTo(TPKey id, object target);

        #endregion increasable

        #region settable

        bool Set<T>(TPKey id, T value);
        bool Set(TPKey id, object value);

        #endregion settable

        #region list

        int IndexOf<T>(TPKey pid, T item);
        int IndexOf<T>(TPKey pid, Predicate<T> condition);
        T GetByIndex<T>(TPKey pid, int idx);
        bool Add<T>(TPKey id, T value);
        bool Add(TPKey id, object value);
        bool AddRange<T>(TPKey id, List<T> items);
        bool Remove<T>(TPKey id, T item);
        bool Remove(TPKey id, object item);
        bool RemoveAll<T>(TPKey id, Predicate<T> predicate);
        bool RemoveAll<T>(TPKey id, Predicate<T> predicate, out int count);
        bool RemoveAll(TPKey id);
        bool RemoveAll(TPKey id, out int count);
        bool Insert<T>(TPKey id, int idx, T item);
        bool Insert(TPKey id, int idx, object item);
        bool Update(TPKey id, int idx);
        bool Replace<T>(TPKey id, int idx, T item);
        bool Swap<T>(TPKey id, int idxA, int idxB);

        #endregion list

        #endregion
    }

    /// <summary>
    /// 属性化对象
    /// </summary>
    public abstract class PropertiedObj<TPKey> : UniqueObj<long>, IPropertiedObj<TPKey>
    {
        /// <summary>
        ///     属性体
        /// </summary>
        private PropertyBody<TPKey> _propertyBody;
        public PropertyBody<TPKey> PropertyBody
        {
            get
            {
                if (_propertyBody != null) return _propertyBody;
                _propertyBody = Create<PropertyBody<TPKey>>(new PropertyBodyArg(this));
                return _propertyBody;
            }
        }

        public IBlock<TPKey> GetBlock(TPKey key)
        {
            return PropertyBody.GetBlock(key);
        }

        public IEnumerable<string> GetFeilds()
        {
            return PropertyBody.Where(x => x.Synchronizable).Select(x => x.RedisFeild);
        }

        public IEnumerable<IBlock<TPKey>> Blocks
        {
            get { return PropertyBody; }
        }

        public abstract string RedisFeild { get;  }

        /// <summary>
        ///     执行初始化
        /// </summary>
        protected override void OnInit(ObjArg objArg)
        {
            base.OnInit(objArg);
            
            // 应用属性
            var more = objArg as PropertiedObjArg<TPKey>;
            if (more != null)
                ApplyProperties(more.Properties);
        }

        /// <summary>
        ///     应用属性
        /// </summary>
        /// <param name="blocks"></param>
        private void ApplyProperties(IEnumerable<Pair<TPKey, IBlock<TPKey>>> blocks)
        {
            if(blocks == null) return;

            foreach (var block in blocks)
            {
                PropertyBody.Inject(block.Value);
            }
        }

        #region 属性监听
        /// <summary>
        ///     监听本对象的某些属性改变
        /// </summary>
        protected Dictionary<TPKey, List<Action<IPropertiedObj<TPKey>, IBlock<TPKey>>>> Listeners
            = new Dictionary<TPKey, List<Action<IPropertiedObj<TPKey>, IBlock<TPKey>>>>();

        /// <summary>
        ///     监听本对象的任何属性改变
        /// </summary>
        protected List<Action<IPropertiedObj<TPKey>, IBlock<TPKey>>> GreedyListeners
            = new List<Action<IPropertiedObj<TPKey>, IBlock<TPKey>>>();

        /// <summary>
        ///     通知属性修改
        /// </summary>
        /// <param name="block"></param>
        public void NotifyPropertyChanged(IBlock<TPKey> block)
        {
            // 通知自己
            DoPropertyChangedNotification(block);

            // 通知任意属性修改监听者
            var greedyActions = GreedyListeners.ToArray();
            foreach (var action in greedyActions)
                action(this, block);

            // 通知某属性修改监听者
            var pid = block.Id;
            if (Listeners.ContainsKey(pid))
            {
                var actions = Listeners[pid];
                var copy = new Action<IPropertiedObj<TPKey>, IBlock<TPKey>>[actions.Count];
                Listeners[pid].CopyTo(copy);

                foreach (var action in copy)
                    action(this, block);
            }

            // 全局（类型监听者）通知
            var publisher = GetAncestor<IPropertyPublishable<TPKey>>();
            if(publisher != null)
                publisher.Publisher.Publish(this, block);
        }

        /// <summary>
        ///     执行属性改变通知
        /// </summary>
        /// <param name="block"></param>
        protected virtual void DoPropertyChangedNotification(IBlock<TPKey> block)
        {
            OnPropertyChanged(block);
        }

        /// <summary>
        ///     通知属性改变
        /// </summary>
        /// <param name="block"></param>
        protected virtual void OnPropertyChanged(IBlock<TPKey> block)
        {
        }
        
        public void Listen(Action<IPropertiedObj<TPKey>, IBlock<TPKey>> handler, params TPKey[] pids)
        {
            if (pids.Length == 0)
            {
                // 监听所有属性改变
                if(!GreedyListeners.Contains(handler))
                    GreedyListeners.Add(handler);
            }
            else
            {
                // ensure this obj contains property : pid
                if (!pids.All(x => PropertyBody.Contains(x))) return;
                foreach (var pid in pids)
                {
                    if (!Listeners.ContainsKey(pid)) Listeners.Add(pid, new List<Action<IPropertiedObj<TPKey>, IBlock<TPKey>>>());

                    var lst = Listeners[pid];
                    if (!lst.Contains(handler))
                        lst.Add(handler);
                    else
                        Logger.Instance.WarnFormat("Handler {0} already registered for {1} of go {2}!", handler, pid, Name);
                }
            }
        }

        public void Unlisten(Action<IPropertiedObj<TPKey>, IBlock<TPKey>> handler, params TPKey[] pids)
        {
            if (pids.Length == 0)
            {
                GreedyListeners.Remove(handler);
            }
            else
            {
                foreach (var pid in pids.Where(pid => Listeners.ContainsKey(pid)))
                {
                    Listeners[pid].Remove(handler);
                }
            }
        }
        #endregion

        #region 属性操作
        public T Get<T>(TPKey id)
        {
            T ret;
            return PropertyBody.Get(id, out ret) ? ret : default(T);
        }

        public bool Get<T>(TPKey id, out T value)
        {
            if (PropertyBody.Get(id, out value)) return true;

            Logger.Instance.WarnFormat("Pid : {0} of {1} not exist!", id, Name);
            return false;
        }

        public List<T> GetList<T>(TPKey id)
        {
            var lst = Get<List<ListItemRepresentation<T>>>(id);
            return lst != null ? lst.Select(x => x.Item).ToList() : null;
        }

        public bool Inject(IBlock<TPKey> block)
        {
            return PropertyBody.Inject(block);
        }

        //public bool Inject(TPKey pid)
        //{
        //    var block = BlockGenerator(PropertyBody, pid);
        //    return block != null && PropertyBody.Inject(block);
        //}

        //public bool Inject(TPKey pid, EBlockMode mode)
        //{
        //    var block = BlockGenerator(PropertyBody, pid);
        //    if (block == null) return false;

        //    block.SetMode(mode);
        //    return PropertyBody.Inject(block);
        //}

        //public bool MultiInject(params TPKey[] pids)
        //{
        //    return pids.Select(pid => BlockGenerator(PropertyBody, pid))
        //        .Aggregate(true, (current, block) => current & PropertyBody.Inject(block));
        //}

        public void NotifyPropertyChanged(TPKey pid)
        {
            if (Started)
                NotifyPropertyChanged(PropertyBody.Blocks[pid]);
        }

        public void NotifyPropertyChanged()
        {
            if (!Started) return;

            foreach (var block in PropertyBody)
            {
                NotifyPropertyChanged(block);
            }
        }

        #region increasable
        public bool Inc<T>(TPKey id, T delta)
        {
            T overflow;
            return Inc(id, delta, out overflow);
        }
        public bool Inc(TPKey id, object delta)
        {
            object overflow;
            return Inc(id, delta, out overflow);
        }
        public bool Inc<T>(TPKey id, T delta, out T overflow)
        {
            if (!PropertyBody.Inc(id, delta, out overflow)) return false;
            NotifyPropertyChanged(id);
            return true;
        }
        public bool Inc(TPKey id, object delta, out object overflow)
        {
            if (!PropertyBody.Inc(id, delta, out overflow)) return false;
            NotifyPropertyChanged(id);
            return true;
        }

        public bool IncTo<T>(TPKey id, T target)
        {
            if (!PropertyBody.IncTo(id, target)) return false;
            NotifyPropertyChanged(id);
            return true;
        }

        public bool IncTo(TPKey id, object target)
        {
            if (!PropertyBody.IncTo(id, target)) return false;
            NotifyPropertyChanged(id);
            return true;
        }
        #endregion increasable

        #region settable
        public bool Set<T>(TPKey id, T value)
        {
            if (!PropertyBody.Set(id, value)) return false;
            NotifyPropertyChanged(id);
            return true;
        }
        public bool Set(TPKey id, object value)
        {
            if (!PropertyBody.Set(id, value)) return false;
            NotifyPropertyChanged(id);
            return true;
        }
        #endregion settable

        #region list

        public int IndexOf<T>(TPKey pid, T item)
        {
            return PropertyBody.IndexOf<T>(pid, item);
        }

        public int IndexOf<T>(TPKey pid, Predicate<T> condition)
        {
            return PropertyBody.IndexOf<T>(pid, condition);
        }

        public T GetByIndex<T>(TPKey pid, int idx)
        {
            return PropertyBody.GetByIndex<T>(pid, idx);
        }

        public bool Add<T>(TPKey id, T value)
        {
            if (!PropertyBody.Add(id, value)) return false;
            NotifyPropertyChanged(id);
            return true;
        }
        public bool Add(TPKey id, object value)
        {
            if (!PropertyBody.Add(id, value)) return false;
            NotifyPropertyChanged(id);
            return true;
        }
        public bool AddRange<T>(TPKey id, List<T> items)
        {
            if (!PropertyBody.MultiAdd(id, items)) return false;
            NotifyPropertyChanged(id);
            return true;
        }
        public bool Remove<T>(TPKey id, T item)
        {
            if (!PropertyBody.Remove(id, item)) return false;
            NotifyPropertyChanged(id);
            return true;
        }
        public bool Remove(TPKey id, object item)
        {
            if (!PropertyBody.Remove(id, item)) return false;
            NotifyPropertyChanged(id);
            return true;
        }

        public bool RemoveAll<T>(TPKey id, Predicate<T> predicate)
        {
            int cnt;
            return RemoveAll<T>(id, predicate, out cnt);
        }

        public bool RemoveAll<T>(TPKey id, Predicate<T> predicate, out int count)
        {
            if (!PropertyBody.RemoveAll(id, predicate, out count)) return false;
            NotifyPropertyChanged(id);
            return true;
        }

        public bool RemoveAll(TPKey id)
        {
            int cnt;
            return RemoveAll(id, out cnt);
        }

        public bool RemoveAll(TPKey id, out int count)
        {
            if (!PropertyBody.RemoveAll(id, out count)) return false;
            NotifyPropertyChanged(id);
            return true;
        }

        public bool Insert<T>(TPKey id, int idx, T item)
        {
            if (!PropertyBody.Insert(id, idx, item)) return false;
            NotifyPropertyChanged(id);
            return true;
        }

        public bool Insert(TPKey id, int idx, object item)
        {
            if (!PropertyBody.Insert(id, idx, item)) return false;
            NotifyPropertyChanged(id);
            return true;
        }

        public bool Update(TPKey id, int idx)
        {
            if (!PropertyBody.Update(id, idx)) return false;
            NotifyPropertyChanged(id);
            return true;
        }

        public bool Replace<T>(TPKey id, int idx, T item)
        {
            if (!PropertyBody.Replace(id, idx, item)) return false;
            NotifyPropertyChanged(id);
            return true;
        }

        public bool Swap<T>(TPKey id, int idxA, int idxB)
        {
            if (!PropertyBody.Swap<T>(id, idxA, idxB)) return false;
            NotifyPropertyChanged(id);
            return true;
        }
        #endregion list
        #endregion
    }   
}