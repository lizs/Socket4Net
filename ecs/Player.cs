using socket4net;

namespace ecs
{
    public class PlayerArg : UniqueObjArg<long>
    {
        public PlayerArg(IObj owner, long key, IRpcSession session, bool syncEnabled, bool persistEnabled)
            : base(owner, key)
        {
            SyncEnabled = syncEnabled;
            PersistEnabled = persistEnabled;
            Session = session;
        }

        public bool SyncEnabled { get; private set; }
        public bool PersistEnabled { get; private set; }
        public IRpcSession Session { get; private set; }
    }

    /// <summary>
    ///     玩家
    ///     挂载系统如下：
    ///     1、 实体系统
    ///     2、 同步系统
    ///     3、 存储系统
    /// </summary>
    public abstract class Player : UniqueObj<long>
    {
        public EntitySys Es { get; protected set; }
        public PersistSys Ps { get; private set; }
        public SyncSys Ss { get; private set; }

        public IRpcSession Session { get; private set; }
        protected override void OnInit(ObjArg objArg)  
        {
            base.OnInit(objArg);

            Es = CreateEntitySys();

            var more = objArg.As<PlayerArg>();

            Session = more.Session;
            if (more.PersistEnabled)
                Ps = Create<PersistSys>(new DataSysArg(this, Es));

            if (more.SyncEnabled)
                Ss = Create<SyncSys>(new DataSysArg(this, Es));
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if(Ss != null)
                Ss.Destroy();

            if(Ps != null)
                Ps.Destroy();

            Es.Destroy();
        }

        protected abstract EntitySys CreateEntitySys();
    }

    public abstract class Player<T> : Player, IFlushable where T : UniqueObj<short>, IAsyncRedisClient
    {
        protected abstract T RedisClient { get; }
        public async void Flush()
        {
            if (Ps != null)
                await Ps.PersistAsync(RedisClient);

            if (Ss != null)
                Ss.Sync(Session);
        }
    }
}
