using System;
using Core.Log;
using Core.Net.TCP;
using Core.RPC;
using Core.Service;

namespace Core
{
    public class CoreConfig
    {
        public string Ip { get; set; }
        public ushort Port { get; set; }

        public int NetServiceCapacity { get; set; }
        public int StaServiceCapacity { get; set; }
        public int ServiceWaitPeriod { get; set; }

        public ILog Logger { get; set; }
        public IService StaService { get; set; }

        public CoreConfig(string ip, ushort port = 5555)
        {
            Ip = ip;
            Port = port;

            NetServiceCapacity = 10000;
            StaServiceCapacity = 10000;

            ServiceWaitPeriod = 10;
            Logger = new DefaultLogger();
        }
    }

    public static class Launcher
    {
        public static ITcpServer Server;
        public static ITcpClient Client;

        public static IService StaService;
        public static IService NetService;

        public static void LaunchServer(CoreConfig cfg)
        {
            Logger.Instance = cfg.Logger;

            Server = new TcpServer<RpcSession>(cfg.Ip, cfg.Port);

            StaService = cfg.StaService ?? new StaService();
            StaService.Startup(cfg.StaServiceCapacity, cfg.ServiceWaitPeriod);

            NetService = new NetService();
            NetService.Startup(cfg.StaServiceCapacity, cfg.ServiceWaitPeriod);

            Server.Startup();
        }

        public static void LaunchClient(CoreConfig cfg)
        {
            Logger.Instance = cfg.Logger;

            Client = new TcpClient<RpcSession>(cfg.Ip, cfg.Port);

            StaService = cfg.StaService ?? new StaService();
            StaService.Startup(cfg.StaServiceCapacity, cfg.ServiceWaitPeriod);

            NetService = new NetService();
            NetService.Startup(cfg.StaServiceCapacity, cfg.ServiceWaitPeriod);
        }

        public static void Shutdown()
        {
            if(Server != null)
                Server.Shutdown();

            if(Client != null)
                Client.Shutdown();

            NetService.Shutdown(true);
            StaService.Shutdown(true);
        }

        public static void PerformInSta(Action action)
        {
            StaService.Perform(action);
        }

        public static void PerformInSta<T>(Action<T> action, T param)
        {
            StaService.Perform(action, param);
        }


        public static void PerformInNet(Action action)
        {
            NetService.Perform(action);
        }

        public static void PerformInNet<T>(Action<T> action, T param)
        {
            NetService.Perform(action, param);
        }
    }
}
