

using System;
using socket4net;

namespace ChatS
{
    class Program
    {
        static void Main(string[] args)
        {
            Obj.Create<Launcher>(new LauncherArg(null, new CustomLog.Log4Net("log4net.config", "ChatS")));
            Launcher.Instance.Start();
            
            // Rpc示例
            RunServer();

            Launcher.Instance.Destroy();
        }

        /// <summary>
        /// Rpc服务器示例
        /// </summary>
        private static void RunServer()
        {
            // 创建服务器
            var server = Obj.Create<Server>(new ServerArg(null, "0.0.0.0", 843));
            
            // 监听事件
            server.EventSessionClosed +=
                (session, reason) => Logger.Instance.InfoFormat("{0} disconnected by {1}", session.Id, reason);

            server.EventSessionEstablished += session => Logger.Instance.InfoFormat("{0} connected", session.Id);

            // 启动
            server.Start();

            // 结束服务器
            var stop = false;
            while (!stop)
            {
                var msg = Console.ReadLine();
                if (string.IsNullOrEmpty(msg)) continue;

                switch (msg.ToUpper())
                {
                    case "QUIT":
                    case "EXIT":
                        server.LogicService.Perform(server.Destroy);
                        stop = true;
                        break;
                }
            }
        }
    }
}
