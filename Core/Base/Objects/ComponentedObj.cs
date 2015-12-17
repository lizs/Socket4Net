using System.Collections;
using System.Collections.Generic;
using socket4net.Util;

namespace socket4net
{
    public class ComponentedObjArg<TPKey> : PropertiedObjArg<TPKey>
    {
        public ComponentedObjArg(IObj parent, long key, IEnumerable<Pair<TPKey, IBlock<TPKey>>> properties)
            : base(parent, key, properties)
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
        
        #region 初始化、卸载

        /// <summary>
        ///     初始化
        /// </summary>
        /// <param name="objArg"></param>
        protected override void OnInit(ObjArg objArg)
        {
            base.OnInit(objArg);

            // 添加组件
            SpawnComponents();
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
                           Create<ComponentsMgr<TPKey>>(new UniqueMgrArg(this)));
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

        public T AddComponent<T>() where T : Component<TPKey>, new()
        {
            return Components.AddComponent<T>();
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