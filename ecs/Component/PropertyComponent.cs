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
using System.Collections.Generic;
using System.Linq;
using socket4net;

namespace ecs
{
    /// <summary>
    ///     内置属性组件
    /// </summary>
    [ComponentId((short)EInternalComponentId.Property)]
    public class PropertyComponent : Component, IProperty
    {
        /// <summary>
        ///     属性体
        /// </summary>
        private PropertyBody _propertyBody;
        public PropertyBody PropertyBody
        {
            get
            {
                if (_propertyBody != null) return _propertyBody;
                _propertyBody = New<PropertyBody>(new PropertyBodyArg(this));
                return _propertyBody;
            }
        }

        /// <summary>
        ///     获取属性块
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IBlock GetBlock(short key)
        {
            return PropertyBody.GetBlock(key);
        }
        
        /// <summary>
        ///     枚举属性块
        /// </summary>
        public IEnumerable<IBlock> Blocks
        {
            get { return PropertyBody; }
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
        protected Dictionary<short, List<Action<IEntity, IBlock>>> Listeners
            = new Dictionary<short, List<Action<IEntity, IBlock>>>();

        /// <summary>
        ///     监听本对象的任何属性改变
        /// </summary>
        protected List<Action<IEntity, IBlock>> GreedyListeners
            = new List<Action<IEntity, IBlock>>();

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
                var copy = new Action<IEntity, IBlock>[actions.Count];
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

        public void Listen(Action<IEntity, IBlock> handler, params short[] pids)
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
                    if (!Listeners.ContainsKey(pid)) Listeners.Add(pid, new List<Action<IEntity, IBlock>>());

                    var lst = Listeners[pid];
                    if (!lst.Contains(handler))
                        lst.Add(handler);
                    else
                        Logger.Ins.Warn("Handler {0} already registered for {1} of go {2}!", handler, pid, Name);
                }
            }
        }

        public void Unlisten(Action<IEntity, IBlock> handler, params short[] pids)
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

        public bool Apply(IReadOnlyCollection<IBlock> blocks)
        {
            return PropertyBody.Apply(blocks);
        }

        public T Get<T>(short pid)
        {
            T ret;
            return PropertyBody.Get(pid, out ret) ? ret : default(T);
        }

        public bool Get<T>(short pid, out T value)
        {
            if (PropertyBody.Get(pid, out value)) return true;

            Logger.Ins.Warn("Pid : {0} of {1} not exist!", pid, Name);
            return false;
        }

        public List<T> GetList<T>(short pid)
        {
            var lst = Get<List<ListItemRepresentation<T>>>(pid);
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

        public bool Inc<T>(short pid, T delta)
        {
            T overflow;
            return Inc(pid, delta, out overflow);
        }

        public bool Inc(short pid, object delta)
        {
            object overflow;
            return Inc(pid, delta, out overflow);
        }

        public bool Inc<T>(short pid, T delta, out T overflow)
        {
            if (!PropertyBody.Inc(pid, delta, out overflow)) return false;
            NotifyPropertyChanged(pid);
            return true;
        }

        public bool Inc(short pid, object delta, out object overflow)
        {
            if (!PropertyBody.Inc(pid, delta, out overflow)) return false;
            NotifyPropertyChanged(pid);
            return true;
        }

        public bool IncTo<T>(short pid, T target)
        {
            if (!PropertyBody.IncTo(pid, target)) return false;
            NotifyPropertyChanged(pid);
            return true;
        }

        public bool IncTo(short pid, object target)
        {
            if (!PropertyBody.IncTo(pid, target)) return false;
            NotifyPropertyChanged(pid);
            return true;
        }

        #endregion increasable

        #region settable

        public bool Set<T>(short pid, T value)
        {
            if (!PropertyBody.Set(pid, value)) return false;
            NotifyPropertyChanged(pid);
            return true;
        }

        public bool Set(short pid, object value)
        {
            if (!PropertyBody.Set(pid, value)) return false;
            NotifyPropertyChanged(pid);
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

        public bool Add<T>(short pid, T value)
        {
            if (!PropertyBody.Add(pid, value)) return false;
            NotifyPropertyChanged(pid);
            return true;
        }

        public bool Add(short pid, object value)
        {
            if (!PropertyBody.Add(pid, value)) return false;
            NotifyPropertyChanged(pid);
            return true;
        }

        public bool AddRange<T>(short pid, List<T> items)
        {
            if (!PropertyBody.MultiAdd(pid, items)) return false;
            NotifyPropertyChanged(pid);
            return true;
        }

        public bool Remove<T>(short pid, T item)
        {
            if (!PropertyBody.Remove(pid, item)) return false;
            NotifyPropertyChanged(pid);
            return true;
        }

        public bool Remove(short pid, object item)
        {
            if (!PropertyBody.Remove(pid, item)) return false;
            NotifyPropertyChanged(pid);
            return true;
        }

        public bool RemoveAll<T>(short pid, Predicate<T> predicate)
        {
            int cnt;
            return RemoveAll(pid, predicate, out cnt);
        }

        public bool RemoveAll<T>(short pid, Predicate<T> predicate, out int count)
        {
            if (!PropertyBody.RemoveAll(pid, predicate, out count)) return false;
            NotifyPropertyChanged(pid);
            return true;
        }

        public bool RemoveAll(short pid)
        {
            int cnt;
            return RemoveAll(pid, out cnt);
        }

        public bool RemoveAll(short pid, out int count)
        {
            if (!PropertyBody.RemoveAll(pid, out count)) return false;
            NotifyPropertyChanged(pid);
            return true;
        }

        public bool Insert<T>(short pid, int idx, T item)
        {
            if (!PropertyBody.Insert(pid, idx, item)) return false;
            NotifyPropertyChanged(pid);
            return true;
        }

        public bool Insert(short pid, int idx, object item)
        {
            if (!PropertyBody.Insert(pid, idx, item)) return false;
            NotifyPropertyChanged(pid);
            return true;
        }

        public bool Replace<T>(short pid, int idx, T item)
        {
            if (!PropertyBody.Replace(pid, idx, item)) return false;
            NotifyPropertyChanged(pid);
            return true;
        }

        public bool Swap<T>(short pid, int idxA, int idxB)
        {
            if (!PropertyBody.Swap<T>(pid, idxA, idxB)) return false;
            NotifyPropertyChanged(pid);
            return true;
        }

        #endregion list

        #endregion
    }
}
