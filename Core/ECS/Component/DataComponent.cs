using System;
using System.Collections.Generic;
using System.Linq;

namespace socket4net
{
    /// <summary>
    ///     内置属性组件
    /// </summary>
    public class DataComponent : Component, IData
    {
        /// <summary>
        ///     属性id格式化方法
        /// </summary>
        public Func<string, short> KeyFormator { get; set; }

        /// <summary>
        ///     属性体
        /// </summary>
        private PropertyBody _propertyBody;
        public PropertyBody PropertyBody
        {
            get
            {
                if (_propertyBody != null) return _propertyBody;
                _propertyBody = Create<PropertyBody>(new PropertyBodyArg(this));
                return _propertyBody;
            }
        }

        public IBlock GetBlock(short key)
        {
            return PropertyBody.GetBlock(key);
        }

        public IEnumerable<string> Feilds
        {
            get { return PropertyBody.Where(x => x.Synchronizable).Select(x => x.RedisFeild); }
        }

        public IEnumerable<IBlock> Blocks
        {
            get { return PropertyBody; }
        }

        /// <summary>
        ///     执行初始化
        /// </summary>
        protected override void OnInit(ObjArg objArg)
        {
            base.OnInit(objArg);

            //// 应用属性
            //var more = objArg.As<PropertiedObjArg>();
            //Apply(more.Properties);
        }

        /// <summary>
        ///     注入属性
        /// </summary>
        /// <param name="blocks"></param>
        public void Inject(IEnumerable<IBlock> blocks)
        {
            if (blocks == null) return;

            foreach (var block in blocks)
            {
                PropertyBody.Inject(block);
            }
        }

        #region 属性监听
        /// <summary>
        ///     监听本对象的某些属性改变
        /// </summary>
        protected Dictionary<short, List<Action<Entity, IBlock>>> Listeners
            = new Dictionary<short, List<Action<Entity, IBlock>>>();

        /// <summary>
        ///     监听本对象的任何属性改变
        /// </summary>
        protected List<Action<Entity, IBlock>> GreedyListeners
            = new List<Action<Entity, IBlock>>();

        /// <summary>
        ///     通知属性修改
        /// </summary>
        /// <param name="block"></param>
        public void NotifyPropertyChanged(IBlock block)
        {
            // 通知自己
            OnPropertyChanged(block);

            // 通知任意属性修改监听者
            var greedyActions = GreedyListeners.ToArray();
            foreach (var action in greedyActions)
                action(Host, block);

            // 通知某属性修改监听者
            var pid = block.Id;
            if (Listeners.ContainsKey(pid))
            {
                var actions = Listeners[pid];
                var copy = new Action<Entity, IBlock>[actions.Count];
                Listeners[pid].CopyTo(copy);

                foreach (var action in copy)
                    action(Host, block);
            }

            // 全局（类型监听者）通知
            //var publisher = GetAncestor<IPropertyPublishable>();
            //if (publisher != null)
            //    publisher.Publisher.Publish(Host, block);
        }


        /// <summary>
        ///     通知属性改变
        /// </summary>
        /// <param name="block"></param>
        protected virtual void OnPropertyChanged(IBlock block)
        {
        }

        public void Listen(Action<Entity, IBlock> handler, params short[] pids)
        {
            if (pids.Length == 0)
            {
                // 监听所有属性改变
                if (!GreedyListeners.Contains(handler))
                    GreedyListeners.Add(handler);
            }
            else
            {
                // ensure this obj contains property : pid
                if (!pids.All(x => PropertyBody.Contains(x))) return;
                foreach (var pid in pids)
                {
                    if (!Listeners.ContainsKey(pid)) Listeners.Add(pid, new List<Action<Entity, IBlock>>());

                    var lst = Listeners[pid];
                    if (!lst.Contains(handler))
                        lst.Add(handler);
                    else
                        Logger.Instance.WarnFormat("Handler {0} already registered for {1} of go {2}!", handler, pid, Name);
                }
            }
        }

        public void Unlisten(Action<Entity, IBlock> handler, params short[] pids)
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
        public T Get<T>(short id)
        {
            T ret;
            return PropertyBody.Get(id, out ret) ? ret : default(T);
        }

        public bool Get<T>(short id, out T value)
        {
            if (PropertyBody.Get(id, out value)) return true;

            Logger.Instance.WarnFormat("Pid : {0} of {1} not exist!", id, Name);
            return false;
        }

        public List<T> GetList<T>(short id)
        {
            var lst = Get<List<ListItemRepresentation<T>>>(id);
            return lst != null ? lst.Select(x => x.Item).ToList() : null;
        }

        public bool Inject(IBlock block)
        {
            return PropertyBody.Inject(block);
        }

        public void NotifyPropertyChanged(short pid)
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

        public bool Inc<T>(short id, T delta)
        {
            T overflow;
            return Inc(id, delta, out overflow);
        }

        public bool Inc(short id, object delta)
        {
            object overflow;
            return Inc(id, delta, out overflow);
        }

        public bool Inc<T>(short id, T delta, out T overflow)
        {
            if (!PropertyBody.Inc(id, delta, out overflow)) return false;
            NotifyPropertyChanged(id);
            return true;
        }

        public bool Inc(short id, object delta, out object overflow)
        {
            if (!PropertyBody.Inc(id, delta, out overflow)) return false;
            NotifyPropertyChanged(id);
            return true;
        }

        public bool IncTo<T>(short id, T target)
        {
            if (!PropertyBody.IncTo(id, target)) return false;
            NotifyPropertyChanged(id);
            return true;
        }

        public bool IncTo(short id, object target)
        {
            if (!PropertyBody.IncTo(id, target)) return false;
            NotifyPropertyChanged(id);
            return true;
        }

        #endregion increasable

        #region settable

        public bool Set<T>(short id, T value)
        {
            if (!PropertyBody.Set(id, value)) return false;
            NotifyPropertyChanged(id);
            return true;
        }

        public bool Set(short id, object value)
        {
            if (!PropertyBody.Set(id, value)) return false;
            NotifyPropertyChanged(id);
            return true;
        }

        #endregion settable

        #region list

        public int IndexOf<T>(short pid, T item)
        {
            return PropertyBody.IndexOf(pid, item);
        }

        public int IndexOf<T>(short pid, Predicate<T> condition)
        {
            return PropertyBody.IndexOf(pid, condition);
        }

        public T GetByIndex<T>(short pid, int idx)
        {
            return PropertyBody.GetByIndex<T>(pid, idx);
        }

        public bool Add<T>(short id, T value)
        {
            if (!PropertyBody.Add(id, value)) return false;
            NotifyPropertyChanged(id);
            return true;
        }

        public bool Add(short id, object value)
        {
            if (!PropertyBody.Add(id, value)) return false;
            NotifyPropertyChanged(id);
            return true;
        }

        public bool AddRange<T>(short id, List<T> items)
        {
            if (!PropertyBody.MultiAdd(id, items)) return false;
            NotifyPropertyChanged(id);
            return true;
        }

        public bool Remove<T>(short id, T item)
        {
            if (!PropertyBody.Remove(id, item)) return false;
            NotifyPropertyChanged(id);
            return true;
        }

        public bool Remove(short id, object item)
        {
            if (!PropertyBody.Remove(id, item)) return false;
            NotifyPropertyChanged(id);
            return true;
        }

        public bool RemoveAll<T>(short id, Predicate<T> predicate)
        {
            int cnt;
            return RemoveAll(id, predicate, out cnt);
        }

        public bool RemoveAll<T>(short id, Predicate<T> predicate, out int count)
        {
            if (!PropertyBody.RemoveAll(id, predicate, out count)) return false;
            NotifyPropertyChanged(id);
            return true;
        }

        public bool RemoveAll(short id)
        {
            int cnt;
            return RemoveAll(id, out cnt);
        }

        public bool RemoveAll(short id, out int count)
        {
            if (!PropertyBody.RemoveAll(id, out count)) return false;
            NotifyPropertyChanged(id);
            return true;
        }

        public bool Insert<T>(short id, int idx, T item)
        {
            if (!PropertyBody.Insert(id, idx, item)) return false;
            NotifyPropertyChanged(id);
            return true;
        }

        public bool Insert(short id, int idx, object item)
        {
            if (!PropertyBody.Insert(id, idx, item)) return false;
            NotifyPropertyChanged(id);
            return true;
        }

        public bool Update(short id, int idx)
        {
            if (!PropertyBody.Update(id, idx)) return false;
            NotifyPropertyChanged(id);
            return true;
        }

        public bool Replace<T>(short id, int idx, T item)
        {
            if (!PropertyBody.Replace(id, idx, item)) return false;
            NotifyPropertyChanged(id);
            return true;
        }

        public bool Swap<T>(short id, int idxA, int idxB)
        {
            if (!PropertyBody.Swap<T>(id, idxA, idxB)) return false;
            NotifyPropertyChanged(id);
            return true;
        }

        #endregion list

        #endregion
    }
}
