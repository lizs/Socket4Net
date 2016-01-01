namespace socket4net
{
    /// <summary>
    ///     玩家
    ///     挂载系统如下：
    ///     1、 实体系统
    ///     2、 同步系统
    ///     3、 存储系统
    /// </summary>
    public class Player : UniqueObj<long>
    {
        public EntitySys Es { get; private set; }
        public PersistSys Ps { get; private set; }
        public SyncSys Ss { get; private set; }

        protected override void OnInit(ObjArg objArg)
        {
            base.OnInit(objArg);
        }
    }
}
