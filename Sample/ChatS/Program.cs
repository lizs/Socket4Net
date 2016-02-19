

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
            var server = Obj.Create<Server>(new ServerArg(null, "0.0.0.0", 843));
            
            // 监听事件
            server.EventSessionClosed +=
                (session, reason) => Logger.Ins.Info("{0} 会话断开，原因： {1}", session.Id, reason);

            server.EventSessionEstablished += session => Logger.Ins.Info("{0} 会话建立", session.Id);

            // 启动
            server.Start();

            // 结束服务器
            while (!server.Destroyed)
            {
                var msg = Console.ReadLine();
                if (string.IsNullOrEmpty(msg)) continue;

                switch (msg.ToUpper())
                {
                    case "EXIT":
                        server.LogicService.Perform(server.Destroy);
                        break;
                }
            }
        }
    }
}
