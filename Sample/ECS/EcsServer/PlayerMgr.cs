using socket4net;

namespace Sample
{
    public class PlayerMgr : UniqueMgr<long, Player>
    {
        private static PlayerMgr _ins;
        public static PlayerMgr Ins
        {
            get { return _ins ?? (_ins = Create<PlayerMgr>(new UniqueMgrArg(null))); }
        }
    }
}