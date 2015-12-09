namespace Pi.Core.Business.Base.New
{
    /// <summary>
    ///     拥有固定唯一Id的对象
    /// </summary>
    public abstract class UniqueObj<TId> : RootedObj, IInitializer<Obj, TId>
    {
        /// <summary>
        ///     唯一Id
        /// </summary>
        public TId Id { get; private set; }

        /// <summary>
        ///     初始为id
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="id"></param>
        public void Init(Obj parent, TId id)
        {
            Init(parent);
            Id = id;
        }
    }
}
