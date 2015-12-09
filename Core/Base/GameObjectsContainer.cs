using System;
using System.Collections.Generic;
using System.Linq;

namespace Pi.Core
{
    public enum ContainerOps
    {
        Invalid,
        Add,
        MultiAdd,
        Remove,
        MultiRemove,
    }

    public class GameObjectsContainer : Mgr<long, GameObject>
    {
        public event Action<GameObject, IEnumerable<GameObject>, ContainerOps> EventContainerChanged;

        protected override bool Add(GameObject item)
        {
            if (!base.Add(item)) return false;
            if (EventContainerChanged != null)
                EventContainerChanged(item, null, ContainerOps.Add);
            return true;
        }

        protected override GameObject Remove(long key)
        {
            var victim = base.Remove(key);
            if (victim == null) return null;

            if (EventContainerChanged != null)
                EventContainerChanged(victim, null, ContainerOps.Remove);

            return victim;
        }

        protected override IEnumerable<GameObject> Remove<T>()
        {
            var victims = base.Remove<T>().ToArray();
            if (victims.Length == 0) return victims;

            if (EventContainerChanged != null)
                EventContainerChanged(null, victims, ContainerOps.MultiRemove);

            return victims;
        }
    }
}
