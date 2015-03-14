
using System;
using System.Collections.Generic;
using System.Linq;
using Core.Net.TCP;
using Core.RPC;
using Core.Timer;

namespace ChatC
{
    public static class RobotMgr
    {
        public static string Ip { get; set; }
        public static ushort Port { get; set; }
        private static readonly Dictionary<long, Robot> Robots = new Dictionary<long, Robot>();

        private static Timer _timer = new Timer("RobotMgr", 2000, 20);
        public static void Run()
        {
            _timer.Arrived += RefreshRobots;
            _timer.Start();
        }

        private static bool _destroy;
        private static void RefreshRobots(Timer timer)
        {
            //_destroy = new Random(DateTime.Now.Millisecond).Next(3) == 1;
            if (/*!_destroy && */Robots.Count < 1000)
            {
                ITcpClient client = new TcpClient<RpcSession>(Ip, Port);
                client.EventSessionEstablished += OnSessionEstablished;
                client.Connect();
            }
//             else
//             {
//                 if (Robots.Count < 1) return;
//                 var x = Robots.Keys.ElementAt(Robots.Keys.Count - 1);
//                 Destroy(x);
//             }
        }

        private static void OnSessionEstablished(ISession session, ITcpClient client)
        {
            SessionMgr.Add(session);
            Create(client);
        }

        public static Robot Create(ITcpClient client)
        {
            var robot = new Robot(client);
            robot.Boot();
            Robots.Add(client.Session.Id, robot);

            return robot;
        }

        public static void Destroy(long id)
        {
            if (Robots.ContainsKey(id))
            {
                var robot = Robots[id];
                robot.Dispose();

                Robots.Remove(id);
            }
        }
    }
}
