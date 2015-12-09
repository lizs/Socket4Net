namespace socket4net
{
    public abstract class RootedObjArg : ObjArg
    {
        public IObj Parent { get; private set; }

        protected RootedObjArg(IObj parent)
        {
            Parent = parent;
        }
    }

    public interface IRootedObj : IObj
    {
        IObj Parent { get; }
        T GetAncestor<T>() where T : class, IObj;
        bool IsMyAncestor(IObj obj);
    }

    public abstract class RootedObj : Obj, IRootedObj
    {
        public override void SetArgument(ObjArg arg)
        {
            base.SetArgument(arg);

            var more = Argument as RootedObjArg;
            if(more != null)
                Parent = more.Parent;
        }

        /// <summary>
        ///     父亲
        /// </summary>
        public IObj Parent { get; private set; }

        ///// <summary>
        /////     是否继承层激活
        ///// </summary>
        //public bool StartedInHierarchy 
        //{
        //    get
        //    {
        //        if (Parent == null) return StartedSelf;

        //        var root = Parent as RootedObj;
        //        if (root != null) return root.StartedInHierarchy;
        //        else
        //            return Parent.StartedSelf;
        //    } 
        //}
        
        /// <summary>
        ///     获取指定类型的根（递归获取）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetAncestor<T>() where T : class, IObj
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
        public bool IsMyAncestor(IObj obj)
        {
            var ancestor = Parent;
            while (ancestor is RootedObj)
            {
                if (obj == ancestor) return true;
                ancestor = (Parent as RootedObj).Parent;
            }

            return false;
        }
    }
}