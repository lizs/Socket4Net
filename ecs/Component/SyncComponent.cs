using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using socket4net;

namespace ecs
{
    public enum ESyncOps
    {
        Sync,
    }

    [ComponentId((short)EInternalComponentId.Sync)]
    public class SyncComponent : RpcComponent
    {
        private EntitySys _es;
        private EntitySys Es
        {
            get { return _es ?? (_es = GetAncestor<Player>().Es); }
        }

        public override Task<bool> OnPush(short ops, byte[] data)
        {
            switch ((ESyncOps) ops)
            {
                case ESyncOps.Sync:
                {
                    var lst = PiSerializer.DeserializeValue<List<EntityProto>>(data);
                    if (lst.IsNullOrEmpty()) return Task.FromResult(false);
                    foreach (var proto in lst)
                    {
                        if (proto is EntityDestroyProto)
                        {
                            Es.Destroy(proto.Id);
                        }
                        else if (proto is EntityUpdateProto)
                        {
                            var update = proto as EntityUpdateProto;
                            var entity = Es.Get(update.Id) ??
                                         Es.Create<Entity>(Type.GetType(string.Format("{0},Shared", update.Type)), new EntityArg(Es, update.Id), true);

                            entity.Apply(update.Blocks.Select(x => new Pair<short, byte[]>(x.Pid, x.Data)));
                        }
                    }

                    return Task.FromResult(true);
                }
            }

            return Task.FromResult(false);
        }
    }
}
