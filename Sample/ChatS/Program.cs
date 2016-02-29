

using System;
using socket4net;

namespace ChatS
{
    class Program
    {
        static void Main(string[] args)
        {
            Obj.Create<Launcher>(new LauncherArg(/* new CustomLog.Log4Net("log4net.config", "ChatS") */));
            Launcher.Ins.Start();

            // Rpc示例
            RunServer();

            Launcher.Ins.Destroy();
        }

        /// <summary>
        /// Rpc服务器示例
        /// </summary>
        private static void RunServer()
        {
            // 创建服务器
            var server = Obj.Create<Server>(new ServerArg(null, "0.0.0.0", 6001), true);

            Console.WriteLine("Press any key to exit");
            Console.ReadLine();

            // 结束服务器
            server.LogicService.Perform(server.Destroy);
        }
    }
}
