using System;
using System.Collections;
using System.Collections.Generic;

namespace socket4net
{
    public class EntityArg : UniqueObjArg<Guid>
    {
        public EntityArg(IObj parent, Guid key)
            : base(parent, key)
        {
        }
    }

    /// <summary>
    ///     组件消息通知基类
    /// </summary>
    public class Message
    {
    }

    public interface IEntity : IUniqueObj<Guid>, IProperty, IScheduler
    {
        T GetComponent<T>() where T : Component;
        T GetComponent<T>(short cpId) where T : Component;
        void Listen(Action<IEntity, IBlock> handler, params short[] pids);
        void Unlisten(Action<IEntity, IBlock> handler, params short[] pids);
        void SendMessage(Message msg);
    }

    /// <summary>
    ///    E(ECS)
    /// </summary>
    public partial class Entity : UniqueObj<Guid>, IEntity, IEnumerable<Component>
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

            // 缓存内置组件（可能为空）
            _property = GetComponent<PropertyComponent>();
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
        protected override void OnReset()
        {
            base.OnReset();

            // 组件重置
            Components.Reset();
        }

        /// <summary>
        ///     卸载
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();

            // 销毁组件
            Components.Destroy();
        }

        #endregion

        public void SendMessage(Message msg)
        {
            foreach (var component in Components)
            {
                component.OnMessage(msg);
            }
        }

        #region 组件化实现

        private ComponentsMgr _components;
        public ComponentsMgr Components
        {
            get
            {
                return _components ??
                       (_components =
                           Create<ComponentsMgr>(new UniqueMgrArg(this)));
            }
        }

        public Component GetComponent(short key)
        {
            return Components.Get(key);
        }

        public bool RemoveComponent(short key)
        {
            return Components.Destroy(key);
        }

        public T GetComponent<T>() where T : Component
        {
            return Components.GetFirst<T>();
        }

        public T GetComponent<T>(short cpId) where T : Component
        {
            return Components.Get(cpId) as T;
        }

        public List<T> GetComponents<T>() where T : Component
        {
            return Components.Get<T>();
        }

        public T AddComponent<T>() where T : Component, new()
        {
            return Components.AddComponent<T>();
        }

        public Component AddComponent(Type cpType)
        {
            return Components.AddComponent(cpType);
        }

        public List<short> RemoveComponent<T>() where T : Component
        {
            return Components.Destroy<T>();
        }

        public IEnumerator GetEnumerator()
        {
            return Components.GetEnumerator();
        }

        IEnumerator<Component> IEnumerable<Component>.GetEnumerator()
        {
            return (IEnumerator<Component>)GetEnumerator();
        }

        #endregion

        #region 订阅

        public void Listen(Action<IEntity, IBlock> handler, params short[] pids)
        {
            _property.Listen(handler, pids);
        }

        public void Unlisten(Action<IEntity, IBlock> handler, params short[] pids)
        {
            _property.Unlisten(handler, pids);
        }

        #endregion
    }
}