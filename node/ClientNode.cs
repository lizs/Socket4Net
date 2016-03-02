using System;
using socket4net;

namespace node
{
    public enum ENodeEvent
    {
        Registered,
    }

    public class ClientNode<TSession> : SingleSessionNode<TSession>, IWatchable<ENodeEvent>
        where TSession : class, IRpcSession, new()
    {
        /// <summary>
        ///     节点watch事件
        /// </summary>
        public event Func<ENodeEvent, bool> Watch;

        private bool _registered;
        public bool Registered
        {
            get { return _registered; }
            protected set
            {
                _registered = value;
                if (Watch != null)
                    Watch(ENodeEvent.Registered);
            }
        }

        public bool AutoReconnectEnabled
        {
            get { return (Config as ClientNodeElement).AutoReconnect; }
        }

        public string Ip
        {
            get { return Config.Ip; }
        }

        public ushort Port
        {
            get { return Config.Port; }
        }

        protected override void OnInit(ObjArg arg)
        {
            base.OnInit(arg);
            Peer = New<Client<TSession>>(new ClientArg(null, Ip, Port, AutoReconnectEnabled), false);
        }

        protected override void OnStart()
        {
            base.OnStart();
            (Peer as Obj).Start();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (Peer == null) return;
            (Peer as Obj).Destroy();
        }
    }
}
