using System;
using System.Configuration;
using CustomLog;
using node;
using socket4net;

namespace Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            // 加载app.config
            var map = new ExeConfigurationFileMap { ExeConfigFilename = "Server.exe.config" };
            var config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
            var x = config.GetSection("ServerConfig");
            var section = x as ChatConfig;

            // 启动launcher
            Obj.Create<MyLauncher>(
                new LauncherArg<ChatConfig>(section, new Log4Net(section.LogConfig.File, "Activity")), true);
            
            Console.WriteLine("Press any key to exit!");
            Console.ReadKey();
            Launcher.Ins.Destroy();
        }
    }
}
