namespace socket4net
{
    public abstract class UniqueObjArg<TKey> : ScheduledObjArg
    {
        public TKey Key { get; private set; }

        protected UniqueObjArg(IObj owner, TKey key)
            : base(owner)
        {
            Key = key;
        }
    }

    public interface IUniqueObj<out TKey> : IScheduledObj
    {
        TKey Id { get; }
    }

    /// <summary>
    ///     拥有唯一Id（容器内唯一，不一定是Guid）
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public abstract class UniqueObj<TKey> : ScheduledObj, IUniqueObj<TKey>
    {
        protected override void OnInit(ObjArg objArg)
        {
            base.OnInit(objArg);

            var more = objArg as UniqueObjArg<TKey>;
            Id = more.Key;
        }

        /// <summary>
        ///     唯一Id
        /// </summary>
        public TKey Id { get; protected set; }

        /// <summary>
        ///     名字
        /// </summary>
        public override string Name
        {
            get { return string.Format("{0}:{1}", base.Name, Id); }
        }
    }
}