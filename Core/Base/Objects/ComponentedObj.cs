using System;
using System.Collections;
using System.Collections.Generic;
using socket4net.Util;

namespace socket4net
{
    public class ComponentedObjArg<TPKey> : PropertiedObjArg<TPKey>
    {
        public ComponentedObjArg(IObj parent, long key, IEnumerable<Pair<TPKey, byte[]>> properties,
            Func<PropertyBody<TPKey>, TPKey, IBlock<TPKey>> blockMaker) : base(parent, key, properties, blockMaker)
        {
        }
    }

    public interface IComponentedObj<TPKey> : IPropertiedObj<TPKey>
    {
        T GetComponent<T>() where T : Component<TPKey>;
        T GetComponent<T>(short cpId) where T : Component<TPKey>;
    }

    /// <summary>
    ///    组件化对象
    /// </summary>
    public abstract class ComponentedObj<TPKey> : PropertiedObj<TPKey>,IComponentedObj<TPKey>,
        IEnumerable<Component<TPKey>>
    {
        /// <summary>
        ///     创建组件
        /// </summary>
        protected virtual void SpawnComponents()
        {
        }
        
        protected virtual void OnSubscribe()
        {
        }

        protected virtual void OnUnsubscribe()
        {
        }

        protected virtual void OnInjectProperties()
        {
        }

        #region 初始化、卸载

        protected override void BeforeInit()
        {
            base.BeforeInit();
            
            // 添加组件
            SpawnComponents();

            // 初始化组件管理器
            Components.Init();

            // 属性注入
            OnInjectProperties();
        }

        /// <summary>
        ///     初始化
        /// </summary>
        /// <param name="arg"></param>
        protected override void OnInit()
        {
            base.OnInit();
            
            // 事件订阅
            OnSubscribe();

            // 组件事件订阅
            // 注意：为何组件的事件订阅不是组件自行处理？
            // 因为：组件有可能订阅其它组件的事件，而该订阅目标组件有可能尚未创建！
            Components.Subscribe();
        }

        /// <summary>
        ///     启动
        /// </summary>
        protected override void OnStart()
        {
            base.OnStart();

            // 组件开始
            Components.Start();
        }

        public override void AfterStart()
        {
            base.AfterStart();
            Components.AfterStart();
        }

        /// <summary>
        /// 重置
        /// </summary>
        protected sealed override void OnReset()
        {
            base.OnReset();

            // 组件重置
            Components.Reset();
        }

        /// <summary>
        ///     卸载
        /// </summary>
        protected sealed override void OnDestroy()
        {
            base.OnDestroy();
            
            // 反订阅
            OnUnsubscribe();

            // 销毁组件
            Components.Destroy();
        }

        /// <summary>
        ///     执行属性修改通知
        /// </summary>
        /// <param name="block"></param>
        protected override void DoPropertyChangedNotification(IBlock<TPKey> block)
        {
            base.DoPropertyChangedNotification(block);

            // 通知组件
            foreach (var component in Components)
            {
                component.OnPropertyChanged(block);
            }
        }

        #endregion

        #region 组件化实现

        private ComponentsMgr<TPKey> _components;
        public ComponentsMgr<TPKey> Components
        {
            get
            {
                return _components ??
                       (_components =
                           ObjFactory.Create<ComponentsMgr<TPKey>>(new UniqueMgrArg(this)));
            }
        }

        public Component<TPKey> GetComponent(short key)
        {
            return Components.Get(key);
        }

        public bool RemoveComponent(short key)
        {
            return Components.Destroy(key);
        }

        public T GetComponent<T>() where T : Component<TPKey>
        {
            return Components.GetFirst<T>();
        }

        public T GetComponent<T>(short cpId) where T : Component<TPKey>
        {
            return Components.Get(cpId) as T;
        }

        public List<T> GetComponents<T>() where T : Component<TPKey>
        {
            return Components.Get<T>();
        }

        public T AddComponent<T>(short id) where T : Component<TPKey>, new()
        {
            return Components.AddComponent<T>(id);
        }

        public List<short> RemoveComponent<T>() where T : Component<TPKey>
        {
            return Components.Destroy<T>();
        }

        public IEnumerator GetEnumerator()
        {
            return Components.GetEnumerator();
        }

        IEnumerator<Component<TPKey>> IEnumerable<Component<TPKey>>.GetEnumerator()
        {
            return (IEnumerator<Component<TPKey>>) GetEnumerator();
        }

        #endregion
    }
}