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
            var map = new ExeConfigurationFileMap { ExeConfigFilename = "EcsServer.exe.config" };
            var config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
            var section = config.GetSection("ServerConfig") as ChatConfig;

            // 启动launcher
            Obj.Create<MyLauncher>(
                new LauncherArg<ChatConfig>(section, new Log4Net(section.LogConfig.File, "Activity")), true);

            Launcher.Ins.Destroy();
        }
    }
}
