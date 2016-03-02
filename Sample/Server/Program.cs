using System;
using CustomLog;
using node;
using socket4net;

namespace Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            // App.Config
            var launcherCfg = LauncherConfig.LoadAs<ChatConfig>("Server.exe.config");

            // 创建并启动Launcher
            var arg = new LauncherArg<ChatConfig>(launcherCfg, new Log4Net(launcherCfg.LogConfig.File, "Server"));
            Obj.New<MyLauncher>(arg, true);
            
            Console.WriteLine("Press any key to exit!");
            Console.ReadKey();

            // 销毁Launcher
            Launcher.Ins.Destroy();
        }
    }
}
