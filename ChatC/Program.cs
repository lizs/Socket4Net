using System;
using System.Threading;
using Core.Net.TCP;
using Core.RPC;
using Core.Service;
using Core.Tool;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace ChatC
{
    internal class Program
    {
        private const string DefaultIp = "127.0.0.1";
        private const ushort DefaultPort = 9527;
        public static bool AutoReplyEnabled = false;
        private static log4net.ILog Log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static Chater _chater;

        private static void Main(string[] args)
        {
            var ip = DefaultIp;
            var port = DefaultPort;
            if (args.Length > 0)
            {
                ip = args[0];
                if (args.Length > 1)
                    port = ushort.Parse(args[1]);
                if (args.Length > 2)
                    AutoReplyEnabled = bool.Parse(args[2]);
            }


            StaService.Instance.StartWorking(5000, 1);
            NetService.Startup();
            ITcpClient client = new TcpClient<RpcSession>(ip, port);
            PerformanceCounter.Run();

            if (AutoReplyEnabled)
            {
                StaService.Perform(() =>
                {
                    RobotMgr.Ip = ip;
                    RobotMgr.Port = port;
                    RobotMgr.Run();
                });
                while (true)
                {
                    if (_chater == null)
                    {
                        Thread.Sleep(100);
                    }
                }
            }
            else
            {
                client.EventSessionEstablished += (session, tcpclient) =>
                {
                    SessionMgr.Add(session);
                    _chater = new Chater(session as RpcSession);
                    _chater.Boot();
                };
                client.Connect();

                SessionMgr.EventSessionClosed += (session, reason) => Log.Warn("Session closed by : " + reason);

                while (true)
                {
                    if (_chater == null)
                    {
                        Thread.Sleep(100);
                    }

                    var command = Console.ReadLine();
                    StaService.Perform(()=> _chater.RequestGmCommand(command, false));
                }
            }
        }
    }
}
