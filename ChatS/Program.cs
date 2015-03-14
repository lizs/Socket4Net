

using System;
using Core;
using Core.Net.TCP;
using Core.RPC;
using Core.Tool;

namespace ChatS
{
    public sealed class ChatSession : RpcSession
    {
    }

    class Program
    {
        private const string DefaultIp = "0.0.0.0";
        private const ushort DefaultPort = 9527;

        static void Main(string[] args)
        {
            var ip = DefaultIp;
            var port = DefaultPort;
            if (args.Length > 0)
            {
                ip = args[0];
                if (args.Length > 1)
                    port = ushort.Parse(args[1]);
            }

            var cfg = new CoreConfig(ip, port);
            Launcher.LaunchServer(cfg);
            var server = Launcher.Server;

            PerformanceCounter.Run();

            SessionMgr.EventSessionClosed += (session, reason) => ChaterMgr.Destroy(session.Id);
            SessionMgr.EventSessionEstablished += session => ChaterMgr.Create(session as RpcSession);

            while (true)
            {
                Console.ReadLine();
            }
        }
    }
}
