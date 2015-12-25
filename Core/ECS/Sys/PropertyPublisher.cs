using System;
using System.Collections.Generic;
using System.Linq;

namespace socket4net
{
    /// <summary>
    ///     属性发布器
    ///     注： 用于监听类型属性、全局属性改变
    /// </summary>
    public sealed class PropertyPublisher : Obj
    {
        /// <summary>
        ///     全局监听者，即：监听任何类型的任何属性改变
        /// </summary>
        private readonly List<Action<Entity, IBlock>> _globalListenrs =
            new List<Action<Entity, IBlock>>();

        /// <summary>
        ///     全局类型监听者，即：监听某类型的任何属性改变
        /// </summary>
        private readonly Dictionary<Type, List<Action<Entity, IBlock>>> _globalTypeListeners =
            new Dictionary<Type, List<Action<Entity, IBlock>>>();

        /// <summary> 
        ///     类型监听， 即：监听某个类型的某些属性改变
        /// </summary>
        private readonly Dictionary<Type, Dictionary<short, List<Action<Entity, IBlock>>>>
            _typeListeners =
                new Dictionary<Type, Dictionary<short, List<Action<Entity, IBlock>>>>();


        /// <summary>
        ///     Property中Notify时必须调用该接口
        /// </summary>
        /// <param name="host"></param>
        /// <param name="block"></param>
        public void Publish(Entity host, IBlock block)
        {
#if DEBUG
            using (new AutoWatch(string.Format("通知属性 : {0}", block.Id)))
            {
#endif
                // 全局发布
                // 注：ToList()是为了克隆一个列表，防止回调时破坏迭代器
                _globalListenrs.ToList().ForEach(cb =>
                {
#if DEBUG
                    using (new AutoWatch(string.Format("回调 {0}:{1}", cb.Target, cb.Method.Name)))
                    {
#endif
                        cb(host, block);
#if DEBUG
                    }
#endif
                });

                // 类型发布
                Publish(host.GetType(), host, block);
#if DEBUG
            }
#endif
        }

        /// <summary>
        ///     递归发布类型属性改变
        /// </summary>
        /// <param name="type"></param>
        /// <param name="host"></param>
        /// <param name="block"></param>
        private void Publish(Type type, Entity host, IBlock block)
        {
            while (true)
            {
                if (type == null || type == typeof(Entity))
                    return;

                // type listeners
                if (_typeListeners.ContainsKey(type))
                {
                    var typeDic = _typeListeners[type];
                    if (typeDic.ContainsKey(block.Id))
                    {
                        var cbs = typeDic[block.Id].ToList();
                        cbs.ForEach(cb =>
                        {
#if DEBUG
                        using (new AutoWatch(string.Format("回调 {0}:{1}", cb.Target, cb.Method.Name)))
                        {
#endif
                            cb(host, block);
#if DEBUG
                        }
#endif
                        });
                    }
                }

                // type any listeners
                if (_globalTypeListeners.ContainsKey(type))
                {
                    var cbs = _globalTypeListeners[type].ToList();
                    cbs.ForEach(cb =>
                    {
#if DEBUG
                    using (new AutoWatch(string.Format("回调 {0}:{1}", cb.Target, cb.Method.Name)))
                    {
#endif
                        cb(host, block);
#if DEBUG
                    }
#endif
                    });
                }

                type = type.BaseType;
            }
        }

        /// <summary>
        ///     监听全局属性
        /// </summary>
        /// <param name="handler"></param>
        public void GlobalListen(Action<Entity, IBlock> handler)
        {
            if (!_globalListenrs.Contains(handler))
                _globalListenrs.Add(handler);
            else
                Logger.Instance.WarnFormat("Handler {0} already registered in global publisher!", handler);
        }

        /// <summary>
        ///     取消监听全局属性
        /// </summary>
        /// <param name="handler"></param>
        public void GlobalUnlisten(Action<Entity, IBlock> handler)
        {
            if (!_globalListenrs.Remove(handler))
                Logger.Instance.WarnFormat("Handler {0} not registered in global publisher!", handler);
        }

        /// <summary>
        ///     监听某类型对象的任何属性修改
        /// </summary>
        /// <param name="group"></param>
        /// <param name="handler"></param>
        public void GlobalListen<T>(Action<Entity, IBlock> handler) where T : Entity
        {
            if (!_globalTypeListeners.ContainsKey(typeof(T)))
                _globalTypeListeners.Add(typeof(T), new List<Action<Entity, IBlock>>());

            var listeners = _globalTypeListeners[typeof(T)];
            if (!listeners.Contains(handler))
                listeners.Add(handler);
            else
                Logger.Instance.WarnFormat("Handler {0} already registered for type {2}!", handler, typeof(T));
        }

        /// <summary>
        ///     取消监听某类型对象的任何属性修改
        /// </summary>
        /// <param name="group"></param>
        /// <param name="handler"></param>
        public void GlobalUnlisten<T>(Action<Entity, IBlock> handler) where T : Entity
        {
            if (!_globalTypeListeners.ContainsKey(typeof(T)))
                return;

            var listeners = _globalTypeListeners[typeof(T)];
            listeners.Remove(handler);
        }
        
        /// <summary>
        ///     监听类型属性
        /// </summary>
        /// <param name="group"></param>
        /// <param name="handler"></param>
        /// <param name="pids"></param>
        public void Listen<T>(Action<Entity, IBlock> handler, params short[] pids) where T : Entity
        {
            if (pids.Length == 0) return;

            if (!_typeListeners.ContainsKey(typeof(T)))
                _typeListeners.Add(typeof(T), new Dictionary<short, List<Action<Entity, IBlock>>>());

            var typeDic = _typeListeners[typeof(T)];
            foreach (var pid in pids)
            {
                if (!typeDic.ContainsKey(pid))
                    typeDic.Add(pid, new List<Action<Entity, IBlock>>());

                var listeners = typeDic[pid];
                if (!listeners.Contains(handler))
                    listeners.Add(handler);
                else
                    Logger.Instance.WarnFormat("Handler {0} already registered for {1} of type {2}!", handler, pid, typeof(T));
            }
        }

        /// <summary>
        ///     取消监听类型
        /// </summary>
        /// <param name="group"></param>
        /// <param name="handler"></param>
        /// <param name="pids"></param>
        public void Unlisten<T>(Action<Entity, IBlock> handler, params short[] pids) where T : Entity
        {
            if (pids.Length == 0) return;

            if (!_typeListeners.ContainsKey(typeof(T)))
                return;

            var typeDic = _typeListeners[typeof(T)];
            foreach (var pid in pids.Where(typeDic.ContainsKey))
            {
                typeDic[pid].Remove(handler);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _globalListenrs.Clear();
            _typeListeners.Clear();
            _globalTypeListeners.Clear();
        }
    }
}
