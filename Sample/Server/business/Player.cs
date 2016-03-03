using ecs;
using Shared;
using socket4net;

namespace Sample
{
    public class Player : FlushablePlayer
    {
        protected override EntitySys CreateEntitySys()
        {
            return
                New<EntitySys>(new EntitySysArg(this,
                    BlockMaker.Create,
                    (l, s) => string.Format("{0}:{1}", l, (EPid)s),
                    s =>
                    {
                        var items = s.Split(':');
                        return (short)items[2].To<EPid>();
                    }));
        }

        protected override void OnStart()
        {
            base.OnStart();
            Logger.Ins.Debug("{0} online!", Name);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Logger.Ins.Debug("{0} offline!", Name);
        }

        protected override void SpawnComponents()
        {
            base.SpawnComponents();
            AddComponent<SampleComponent>();
        }
    }
}
