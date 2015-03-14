

using System;
using Core.Net.TCP;
using Core.RPC;
using Core.Tool;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace ChatS
{
    public sealed class ChatSession : RpcSession
    {
    }

    class Program
    {
        private static TcpServer<ChatSession> _server;
        private const string DefaultIp = "0.0.0.0";
        private const ushort DefaultPort = 9527;
        private static log4net.ILog Log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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

            _server = new TcpServer<ChatSession>(ip, port);
            _server.Startup();

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
