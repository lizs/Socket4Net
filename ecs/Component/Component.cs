using System;
using socket4net;
#if NET45
using System.Threading.Tasks;
#endif

namespace ecs
{
    public class ComponentArg : UniqueObjArg<short>
    {
        public ComponentArg(IObj parent, short key)
            : base(parent, key)
        {
        }
    }

    public interface IComponent : IUniqueObj<short>
    {
    }

    /// <summary>
    ///     组件
    ///     游戏逻辑拆分以组件为单元
    /// </summary>
    public abstract class Component : UniqueObj<short>, IComponent
    {
        /// <summary>
        ///     实体爹
        /// </summary>
        public Entity Host
        {
            get { return Owner as Entity; }
        }

        /// <summary>
        ///     获取兄弟组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetComponent<T>() where T : Component
        {
            return GetAncestor<Entity>().GetComponent<T>();
        }

        /// <summary>
        ///     获取兄弟组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cpId"></param>
        /// <returns></returns>
        public T GetComponent<T>(short cpId) where T : Component
        {
            return GetAncestor<Entity>().GetComponent<T>(cpId);
        }

        /// <summary>
        ///     启动
        /// </summary>
        protected override void OnStart()
        {
            base.OnStart();
            OnSubscribe();
        }

        /// <summary>
        ///     卸载
        /// </summary>
        protected override void OnDestroy()
        {
            OnUnsubscribe();
            base.OnDestroy();
        }

        /// <summary>
        ///     订阅
        /// </summary>
        protected virtual void OnSubscribe()
        {
        }

        /// <summary>
        ///     反订阅
        /// </summary>
        protected virtual void OnUnsubscribe()
        {
        }

        /// <summary>
        ///     处理消息
        /// </summary>
        /// <param name="msg"></param>
        public virtual void OnMessage(Message msg)
        {
        }

#if NET45
        public virtual Task<RpcResult> OnMessageAsync(AsyncMsg msg)
        {
            return Task.FromResult(RpcResult.Failure);
        }
#else
        public virtual void OnMessageAsync(AsyncMsg msg, Action<RpcResult> cb)
        {
            
        }
#endif
    }
}