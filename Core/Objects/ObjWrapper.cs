namespace socket4net.objects
{
    /// <summary>
    ///     arg
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObjWrapperArg<T> : ObjArg
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="obj"></param>
        public ObjWrapperArg(IObj owner, T obj) : base(owner)
        {
            Object = obj;
        }

        /// <summary>
        /// 
        /// </summary>
        public T Object { get; }
    }

    /// <summary>
    ///     object wrapper
    ///     make any object schedulable
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObjWrapper<T> : Obj
    {
        /// <summary>
        ///     underline object
        /// </summary>
        public T Object { get; private set; }

        /// <summary>
        ///    internal called when an Obj is initialized
        /// </summary>
        /// <param name="arg"></param>
        protected override void OnInit(ObjArg arg)
        {
            base.OnInit(arg);

            var more = arg as ObjWrapperArg<T>;
            Object = more.Object;
        }
    }
}
