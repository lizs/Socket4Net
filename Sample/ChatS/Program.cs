

using System;
using socket4net;

namespace ChatS
{
    class Program
    {
        static void Main(string[] args)
        {
            // 初始logger（可自定义日志）
            GlobalVarPool.Instance.Set(GlobalVarPool.NameOfLogger, new CustomLog.Log4Net("log4net.config", "ChatS"));
            
            // Rpc示例
            RunServer();
        }

        /// <summary>
        /// Rpc服务器示例
        /// </summary>
        private static void RunServer()
        {
            // 创建服务器
            var server = Obj.Create<Server>(new ServerArg(null, "0.0.0.0", 5000));
            
            // 监听事件
            server.EventSessionClosed +=
                (session, reason) => Logger.Instance.InfoFormat("{0} disconnected by {1}", session.Id, reason);

            server.EventSessionEstablished += session => Logger.Instance.InfoFormat("{0} connected", session.Id);

            // 启动
            server.Start();

            // 结束服务器
            while (true)
            {
                if (Console.ReadKey().Key == ConsoleKey.Q)
                {
                    server.Destroy();
                    break;
                }
            }
        }
    }
}
