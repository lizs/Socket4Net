using System;
using System.Threading;
using Core;
using Core.Log;
using Core.Net.TCP;
using Core.RPC;
using Core.Service;

namespace ChatC
{
    internal class Program
    {
        private const string DefaultIp = "127.0.0.1";
        private const ushort DefaultPort = 9527;
        public static bool AutoReplyEnabled = false;

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


            var cfg = new CoreConfig(ip, port);
            Launcher.LaunchClient(cfg);
            var client = Launcher.Client;
            Core.Tool.PerformanceCounter.Run();


            if (AutoReplyEnabled)
            {
                Launcher.PerformInSta(() =>
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

                SessionMgr.EventSessionClosed += (session, reason) => Logger.Instance.Warn("Session closed by : " + reason);

                while (true)
                {
                    if (_chater == null)
                    {
                        Thread.Sleep(100);
                    }

                    var command = Console.ReadLine();
                    Launcher.PerformInSta(()=> _chater.RequestGmCommand(command, false));
                }
            }
        }
    }
}
