using socket4net;

namespace ecs
{
    public class PlayerMgr : UniqueMgr<long, Player>
    {
        private static PlayerMgr _ins;
        public static PlayerMgr Ins
        {
            get { return _ins ?? (_ins = New<PlayerMgr>(new UniqueMgrArg(null))); }
        }
    }
}