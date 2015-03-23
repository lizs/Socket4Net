using System;
using System.Collections.Generic;
using System.Threading;
using Core.Log;
using Core.Net.TCP;
using Core.Service;
using CustomLog;

namespace PerformanceTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var ip = "127.0.0.1";
            var port = (ushort) 5000;
            var count = 1000;
            if (args.Length > 0)
            {
                ip = args[0];
                if (args.Length > 1) port = ushort.Parse(args[1]);
                if (args.Length > 2) count = int.Parse(args[2]);
            }

            Logger.Instance = new Log4Net();

            var logics = new LogicService { Capacity = 10000, Period = 10 };
            var nets = new NetService { Capacity = 10000, Period = 10 };
            logics.Start();
            nets.Start();

            var monitor = new Monitor<TestSession>();

            for (int i = 0; i < count; i++)
            {
                Thread.Sleep(1);
                var client = new Client<TestSession>(ip, port);
                client.EventSessionEstablished += session => session.DoTest();

                client.Start(nets, logics);

                if (i == 0)
                {
                    // 监视第一个客户端即可
                    // 因为所有客户端都共享网络和逻辑服务
                    monitor.Start(client);
                }
            }

            while (true)
            {
                Console.ReadLine();
            }
        }
    }
}
