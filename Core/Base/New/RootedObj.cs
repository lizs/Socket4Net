namespace Pi.Core.Business.Base.New
{
    /// <summary>
    ///     有根的对象
    ///     即：有父亲、爷爷...
    /// </summary>
    public abstract class RootedObj : Obj, IInitializer<Obj>
    {
        /// <summary>
        ///     父亲
        /// </summary>
        public Obj Parent { get; private set; }

        /// <summary>
        ///     初始父亲
        /// </summary>
        /// <param name="parent"></param>
        public void Init(Obj parent)
        {
            Init();
            Parent = parent;
        }

        /// <summary>
        ///     获取指定类型的根（递归获取）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetAncestor<T>() where T : Obj
        {
            if (this is T) return (this as T);
            if (Parent is T) return (Parent as T);
            if (Parent is RootedObj)
                return (Parent as RootedObj).GetAncestor<T>();

            return null;
        }

        /// <summary>
        ///    是否为祖先
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool IsMyAncestor(Obj obj)
        {
            var ancestor = Parent;
            while (ancestor != null && ancestor is RootedObj)
            {
                if (obj == ancestor) return true;
                ancestor = (Parent as RootedObj).Parent;
            }

            return false;
        }
    }
}
