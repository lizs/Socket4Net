namespace socket4net
{
    public class PeerArg : ObjArg
    {
        public PeerArg(IObj parent, string ip, ushort port, ILogicService logicService, INetService netService)
            : base(parent)
        {
            Ip = ip;
            Port = port;
            LogicService = logicService;
            NetService = netService;
        }

        public string Ip { get; private set; }
        public ushort Port { get; private set; }
        public ILogicService LogicService { get; private set; }
        public INetService NetService { get; private set; }
    }
}