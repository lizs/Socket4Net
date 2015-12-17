namespace socket4net
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
    public abstract class Component<TPKey> : UniqueObj<short>, IComponent
    {
        /// <summary>
        /// 获取兄弟组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetComponent<T>() where T : Component<TPKey>
        {
            var componentedObj = GetAncestor<IComponentedObj<TPKey>>();
            return componentedObj != null ? componentedObj.GetComponent<T>() : default(T);
        }

        public T GetComponent<T>(short cpId) where T : Component<TPKey>
        {
            var componentedObj = GetAncestor<IComponentedObj<TPKey>>();
            return componentedObj != null ? componentedObj.GetComponent<T>(cpId) : default(T);
        }

        /// <summary>
        ///     执行初始化
        /// </summary>
        /// <param name="arg"></param>
        protected override void OnInit(ObjArg objArg)
        {
            base.OnInit(objArg);
            OnInjectProperties();
        }

        /// <summary>
        ///     卸载
        /// </summary>
        protected sealed override void OnDestroy()
        {
            base.OnDestroy();
            OnUnsubscribe();
        }

        /// <summary>
        ///     订阅
        /// </summary>
        public void Subscribe()
        {
            OnSubscribe();
        }

        /// <summary>
        ///   
        /// </summary>
        protected virtual void OnInjectProperties()
        {
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
        ///     Host属性修改通知
        /// </summary>
        /// <param name="block"></param>
        public virtual void OnPropertyChanged(IBlock<TPKey> block)
        {
        }
    }
}