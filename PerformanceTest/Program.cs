using System;
using System.Collections.Generic;
using System.Threading;
using Core.Log;
using Core.Net.TCP;
using Core.RPC;
using Core.Service;
using Proto;

namespace PerformanceTest
{
    internal class TesterMgr
    {
        private readonly Dictionary<long, Tester> _testers = new Dictionary<long, Tester>();

        public Tester Create(Client client)
        {
            var id = client.Session.Id;
            if (_testers.ContainsKey(id)) throw new Exception();

            var ret = new Tester(client);
            ret.Boot();

            _testers.Add(id, ret);

            return ret;
        }

        public void Destroy(long id)
        {
            if (_testers.ContainsKey(id))
            {
                var tester = _testers[id];
                tester.Dispose();

                _testers.Remove(id);
            }
        }
    }

    internal class Tester : RpcHost
    {
        protected override void RegisterRpcHandlers()
        {
        }

        public long Id { get; private set; }
        private readonly Client _client;

        public Tester(Client client)
            : base(client.Session)
        {
            _client = client;
            Id = _client.Session.Id;
            Request();
        }

        private void Request()
        {
            _client.Session.Request(RpcRoute.GmCmd, new Message2Server { Message = "Tester request" }, (success, bytes) =>
            {
                if (success)
                {
                    Request();
                }
                else
                    Logger.Instance.Error("Server response false");
            } );
        }
    }


    class Program
    {
        static TesterMgr _mgr = new TesterMgr();

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

            Logger.Instance = new DefaultLogger();

            var logics = new LogicService { Capacity = 10000, Period = 10 };
            var nets = new NetService { Capacity = 10000, Period = 10 };
            logics.Start();
            nets.Start();

            for (int i = 0; i < count; i++)
            {
                Thread.Sleep(1);
                var client = new Client(ip, port);
                client.EventSessionClosed += (session, reason) => _mgr.Destroy(session.Id);
                client.EventSessionEstablished += session => _mgr.Create(session.HostPeer as Client);

                client.Start(nets, logics);
            }

            while (true)
            {
                Console.ReadLine();
            }
        }
    }
}
