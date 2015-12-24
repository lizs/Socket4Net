using System;
using System.Collections;
using System.Collections.Generic;

namespace socket4net
{
    public class EntityArg : UniqueObjArg<long>
    {
        public EntityArg(IObj parent, long key, short group)
            : base(parent, key)
        {
            Group = group;
        }

        public short Group { get; private set; }
    }

    public interface IEntity : IUniqueObj<long>, IData, IScheduler
    {
        short Group { get; }
        T GetComponent<T>() where T : Component;
        T GetComponent<T>(short cpId) where T : Component;
        void Listen(Action<IEntity, IBlock> handler, params short[] pids);
        void Unlisten(Action<IEntity, IBlock> handler, params short[] pids);
        void SendMessage(object msg);
    }

    /// <summary>
    ///    E(ECS)
    /// </summary>
    public sealed partial class Entity : UniqueObj<long>, IEntity, IEnumerable<Component>
    {
        /// <summary>
        ///     组
        /// </summary>
        public short Group { get; private set; }

        /// <summary>
        ///     创建组件
        /// </summary>
        private void SpawnComponents()
        {
            var cps = ComponentsCache.Instance.Get(GetType());
            if (cps.IsNullOrEmpty()) return;

            foreach (var type in cps)
            {
                AddComponent(type);
            }
        }
        
        #region 初始化、卸载

        /// <summary>
        ///     初始化
        /// </summary>
        /// <param name="objArg"></param>
        protected override void OnInit(ObjArg objArg)
        {
            base.OnInit(objArg);

            // 组
            Group = objArg.As<EntityArg>().Group;

            // 添加组件
            SpawnComponents();

            // 缓存内置组件（可能为空）
            _data = GetComponent<DataComponent>();
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

        public void SendMessage(object msg)
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
            return (IEnumerator<Component>) GetEnumerator();
        }

        #endregion

        #region 订阅
        public void Listen(Action<IEntity, IBlock> handler, params short[] pids)
        {
            _data.Listen(handler, pids);
        }

        public void Unlisten(Action<IEntity, IBlock> handler, params short[] pids)
        {
            _data.Unlisten(handler, pids);
        }

        #endregion
    }
}