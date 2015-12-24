namespace socket4net
{
    public class PeerArg : ObjArg
    {
        public PeerArg(IObj parent, string ip, ushort port)
            : base(parent)
        {
            Ip = ip;
            Port = port;
        }

        public string Ip { get; private set; }
        public ushort Port { get; private set; }
    }
}