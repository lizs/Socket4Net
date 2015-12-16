using System;
using System.Collections.Generic;
using System.Linq;

namespace socket4net
{
    public class PropertyPublisherArg : ObjArg
    {
        public Type Root { get; private set; }

        public PropertyPublisherArg(IObj owner, Type root) : base(owner)
        {
            Root = root;
        }
    }

    /// <summary>
    ///     属性发布器
    ///     注： 用于监听类型属性、全局属性改变
    /// </summary>
    public sealed class PropertyPublisher<TPKey> : Obj
    {
        //private static readonly Type Root = typeof (Go).BaseType;
        private Type Root { get; set; }

        /// <summary>
        ///     全局监听者，即：监听任何类型的任何属性改变
        /// </summary>
        private readonly List<Action<IPropertiedObj<TPKey>, IBlock<TPKey>>> _globalListenrs =
            new List<Action<IPropertiedObj<TPKey>, IBlock<TPKey>>>();

        /// <summary>
        ///     全局类型监听者，即：监听某类型的任何属性改变
        /// </summary>
        private readonly Dictionary<Type, List<Action<IPropertiedObj<TPKey>, IBlock<TPKey>>>> _globalTypeListeners =
            new Dictionary<Type, List<Action<IPropertiedObj<TPKey>, IBlock<TPKey>>>>();

        /// <summary>
        ///     类型监听， 即：监听某个类型的某些属性改变
        /// </summary>
        private readonly Dictionary<Type, Dictionary<TPKey, List<Action<IPropertiedObj<TPKey>, IBlock<TPKey>>>>>
            _typeListeners =
                new Dictionary<Type, Dictionary<TPKey, List<Action<IPropertiedObj<TPKey>, IBlock<TPKey>>>>>();

        /// <summary>
        ///     初始化
        /// </summary>
        /// <param name="objArg"></param>
        protected override void OnInit(ObjArg objArg)
        {
            base.OnInit(objArg);

            var more = objArg as PropertyPublisherArg;
            Root = more.Root;
        }

        /// <summary>
        ///     Property中Notify时必须调用该接口
        /// </summary>
        /// <param name="host"></param>
        /// <param name="block"></param>
        public void Publish(IPropertiedObj<TPKey> host, IBlock<TPKey> block)
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
        private void Publish(Type type, IPropertiedObj<TPKey> host, IBlock<TPKey> block)
        {
            while (true)
            {
                if (type == null || type == Root) // 到Go为止
                    return;

                // type listeners
                if (_typeListeners.ContainsKey(type))
                {
                    var typeDic =
                        _typeListeners[type];
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
        public void GlobalListen(Action<IPropertiedObj<TPKey>, IBlock<TPKey>> handler)
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
        public void GlobalUnlisten(Action<IPropertiedObj<TPKey>, IBlock<TPKey>> handler)
        {
            if (!_globalListenrs.Remove(handler))
                Logger.Instance.WarnFormat("Handler {0} not registered in global publisher!", handler);
        }

        public void GlobalListen<T>(Action<IPropertiedObj<TPKey>, IBlock<TPKey>> handler)
            where T : IPropertiedObj<TPKey>
        {
            GlobalListen(typeof (T), handler);
        }

        public void GlobalUnlisten<T>(Action<IPropertiedObj<TPKey>, IBlock<TPKey>> handler)
            where T : IPropertiedObj<TPKey>
        {
            GlobalUnlisten(typeof (T), handler);
        }

        /// <summary>
        ///     监听某类型对象的任何属性修改
        /// </summary>
        /// <param name="type"></param>
        /// <param name="handler"></param>
        public void GlobalListen(Type type, Action<IPropertiedObj<TPKey>, IBlock<TPKey>> handler)
        {
            if (type == null) return;

            if (!_globalTypeListeners.ContainsKey(type))
                _globalTypeListeners.Add(type, new List<Action<IPropertiedObj<TPKey>, IBlock<TPKey>>>());

            var listeners = _globalTypeListeners[type];
            if (!listeners.Contains(handler))
                listeners.Add(handler);
            else
                Logger.Instance.WarnFormat("Handler {0} already registered for type {2}!", handler, type);
        }

        /// <summary>
        ///     取消监听某类型对象的任何属性修改
        /// </summary>
        /// <param name="type"></param>
        /// <param name="handler"></param>
        public void GlobalUnlisten(Type type, Action<IPropertiedObj<TPKey>, IBlock<TPKey>> handler)
        {
            if (type == null) return;

            if (!_globalTypeListeners.ContainsKey(type))
                return;

            var listeners = _globalTypeListeners[type];
            listeners.Remove(handler);
        }

        /// <summary>
        ///     监听类型属性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handler"></param>
        /// <param name="pids"></param>
        public void Listen<T>(Action<IPropertiedObj<TPKey>, IBlock<TPKey>> handler, params TPKey[] pids)
        {
            Listen(typeof (T), handler, pids);
        }

        /// <summary>
        ///     取消监听类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handler"></param>
        /// <param name="pids"></param>
        public void Unlisten<T>(Action<IPropertiedObj<TPKey>, IBlock<TPKey>> handler, params TPKey[] pids)
        {
            Unlisten(typeof (T), handler, pids);
        }

        /// <summary>
        ///     监听类型属性
        /// </summary>
        /// <param name="type"></param>
        /// <param name="handler"></param>
        /// <param name="pids"></param>
        public void Listen(Type type, Action<IPropertiedObj<TPKey>, IBlock<TPKey>> handler, params TPKey[] pids)
        {
            if (pids.Length == 0 || type == null) return;

            if (!_typeListeners.ContainsKey(type))
                _typeListeners.Add(type, new Dictionary<TPKey, List<Action<IPropertiedObj<TPKey>, IBlock<TPKey>>>>());

            var typeDic = _typeListeners[type];
            foreach (var pid in pids)
            {
                if (!typeDic.ContainsKey(pid))
                    typeDic.Add(pid, new List<Action<IPropertiedObj<TPKey>, IBlock<TPKey>>>());

                var listeners = typeDic[pid];
                if (!listeners.Contains(handler))
                    listeners.Add(handler);
                else
                    Logger.Instance.WarnFormat("Handler {0} already registered for {1} of type {2}!", handler, pid, type);
            }
        }

        /// <summary>
        ///     取消监听类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handler"></param>
        /// <param name="pids"></param>
        public void Unlisten(Type type, Action<IPropertiedObj<TPKey>, IBlock<TPKey>> handler, params TPKey[] pids)
        {
            if (type == null || pids.Length == 0) return;

            if (!_typeListeners.ContainsKey(type))
                return;

            var typeDic = _typeListeners[type];
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
        }
    }
}
