using System;

namespace socket4net
{
    public class ComponentsMgr<TPKey> : UniqueMgr<short, Component<TPKey>>
    {
        /// <summary>
        ///     订阅
        ///     订阅为何这么特殊，请参照：ComponentedObj.Init 中相关说明
        /// </summary>
        public void Subscribe()
        {
            foreach (var cp in this)
            {
                cp.Subscribe();
            }
        }

        public T GetRpcComponent<T>(short id) where T : RpcComponent<TPKey>
        {
            return Get<T>(id);
        }

        public T AddRpcComponent<T>(short id, Func<IRpcSession> sessionGetter) where T : RpcComponent<TPKey>, new()
        {
            var cp = Get(id);
            if (cp != null)
            {
                Logger.Instance.WarnFormat("Component already added : {0}", cp.GetType());
                return null;
            }

            // todo lizs
            cp = Create<T>(new RpcComponentArg(this, id, sessionGetter), false, false);
            return (T)cp;
        }

        public T AddComponent<T>(short id) where T : Component<TPKey>, new()
        {
            var cp = Get(id);
            if (cp != null)
            {
                Logger.Instance.WarnFormat("Component already added : {0}", cp.GetType());
                return null;
            }

            // todo lizs
            cp = Create<T>(new ComponentArg(this, id), false, false);
            return (T)cp;
        }
    }
}
