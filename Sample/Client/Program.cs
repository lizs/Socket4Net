using System;
using System.Linq;
using CustomLog;
using ecs;
using node;
using socket4net;

namespace Sample
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // App.Config
            var launcherCfg = LauncherConfig.LoadAs<ClientConfig>("Client.exe.config");

            // 创建并启动Launcher
            var arg = new LauncherArg<ClientConfig>(launcherCfg, new Log4Net(launcherCfg.LogConfig.File, "Client"));
            Obj.New<MyLauncher>(arg, true);

            Test();

            // 销毁Launcher
            Launcher.Ins.Destroy();
        }

        private static void Test()
        {
            // 结束
            var stopped = false;
            while (!stopped)
            {
                var msg = Console.ReadLine();
                if (string.IsNullOrEmpty(msg)) continue;

                switch (msg.ToUpper())
                {
                    case "QUIT":
                    case "EXIT":
                    {
                        stopped = true;
                        break;
                    }

                    case "ECHO":
                    {
                        var player = PlayerMgr.Ins.FirstOrDefault();
                        var cp = player.GetComponent<SampleComponent>();
                        cp.Echo(msg);
                        break;
                    }

                    case "ADD":
                    {
                        var player = PlayerMgr.Ins.FirstOrDefault();
                        var cp = player.GetComponent<SampleComponent>();
                        cp.Add();
                        break;
                    }

                    case "DEL":
                    {
                        var player = PlayerMgr.Ins.FirstOrDefault();
                        var cp = player.GetComponent<SampleComponent>();
                        cp.Del();
                        break;
                    }

                    default:
                    {
                        var player = PlayerMgr.Ins.FirstOrDefault();
                        var cp = player.GetComponent<SampleComponent>();
                        cp.Broadcast(msg);
                        break;
                    }
                }
            }
        }
    }
}
