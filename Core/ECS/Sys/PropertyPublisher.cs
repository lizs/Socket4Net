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
        private readonly Dictionary<short, List<Action<Entity, IBlock>>> _globalGroupListeners =
            new Dictionary<short, List<Action<Entity, IBlock>>>();

        /// <summary> 
        ///     类型监听， 即：监听某个类型的某些属性改变
        /// </summary>
        private readonly Dictionary<short, Dictionary<short, List<Action<Entity, IBlock>>>>
            _groupListeners =
                new Dictionary<short, Dictionary<short, List<Action<Entity, IBlock>>>>();


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
                Publish(host.Group, host, block);
#if DEBUG
            }
#endif
        }

        /// <summary>
        ///     递归发布类型属性改变
        /// </summary>
        /// <param name="group"></param>
        /// <param name="host"></param>
        /// <param name="block"></param>
        private void Publish(short group, Entity host, IBlock block)
        {
            // type listeners
            if (_groupListeners.ContainsKey(group))
            {
                var typeDic = _groupListeners[group];
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
            if (_globalGroupListeners.ContainsKey(@group))
            {
                var cbs = _globalGroupListeners[@group].ToList();
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
        public void GlobalListen(short group, Action<Entity, IBlock> handler)
        {
            if (!_globalGroupListeners.ContainsKey(group))
                _globalGroupListeners.Add(group, new List<Action<Entity, IBlock>>());

            var listeners = _globalGroupListeners[group];
            if (!listeners.Contains(handler))
                listeners.Add(handler);
            else
                Logger.Instance.WarnFormat("Handler {0} already registered for type {2}!", handler, group);
        }

        /// <summary>
        ///     取消监听某类型对象的任何属性修改
        /// </summary>
        /// <param name="group"></param>
        /// <param name="handler"></param>
        public void GlobalUnlisten(short group, Action<Entity, IBlock> handler)
        {
            if (!_globalGroupListeners.ContainsKey(group))
                return;

            var listeners = _globalGroupListeners[group];
            listeners.Remove(handler);
        }
        
        /// <summary>
        ///     监听类型属性
        /// </summary>
        /// <param name="group"></param>
        /// <param name="handler"></param>
        /// <param name="pids"></param>
        public void Listen(short group, Action<Entity, IBlock> handler, params short[] pids)
        {
            if (pids.Length == 0) return;

            if (!_groupListeners.ContainsKey(group))
                _groupListeners.Add(group, new Dictionary<short, List<Action<Entity, IBlock>>>());

            var typeDic = _groupListeners[group];
            foreach (var pid in pids)
            {
                if (!typeDic.ContainsKey(pid))
                    typeDic.Add(pid, new List<Action<Entity, IBlock>>());

                var listeners = typeDic[pid];
                if (!listeners.Contains(handler))
                    listeners.Add(handler);
                else
                    Logger.Instance.WarnFormat("Handler {0} already registered for {1} of type {2}!", handler, pid, group);
            }
        }

        /// <summary>
        ///     取消监听类型
        /// </summary>
        /// <param name="group"></param>
        /// <param name="handler"></param>
        /// <param name="pids"></param>
        public void Unlisten(short group, Action<Entity, IBlock> handler, params short[] pids)
        {
            if (pids.Length == 0) return;

            if (!_groupListeners.ContainsKey(group))
                return;

            var typeDic = _groupListeners[group];
            foreach (var pid in pids.Where(typeDic.ContainsKey))
            {
                typeDic[pid].Remove(handler);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _globalListenrs.Clear();
            _groupListeners.Clear();
        }
    }
}
