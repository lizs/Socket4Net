

using System;
using Core.Log;
using Core.Net.TCP;
using Core.RPC;
using Core.Service;
using Core.Tool;

namespace ChatS
{
    public sealed class ChatSession : RpcSession
    {
    }

    class Program
    {
        static void Main(string[] args)
        {
            // 初始logger（可自定义日志）
            Logger.Instance = new DefaultLogger();

            // 单服务器示例
            //SingleInsSample();

            // 多服务器示例
            //MultiInsSample();

            // Rpc示例
            RpcSample();
        }

        /// <summary>
        /// Rpc服务器示例
        /// </summary>
        private static void RpcSample()
        {
            // 创建服务器
            var server = new Server("0.0.0.0", 5000);

            // 创建聊天者管理器
            var mgr = new ChaterMgr();

            // 创建监控（可选）
            var moniter = new Monitor();

            // 监听事件
            server.EventSessionClosed +=
                (session, reason) =>
                {
                    Logger.Instance.InfoFormat("{0} disconnected by {1}", session.Id, reason);

                    // 从管理器删除
                    mgr.Destroy(session.Id);
                };

            server.EventSessionEstablished += session =>
            {
                Logger.Instance.InfoFormat("{0} connected", session.Id);

                // 创建一个聊天者
                mgr.Create(session);
            };

            // 启动服务器
            // 注意：该服务器拥有自己独立的网络服务和逻辑服务，故传入参数为null
            server.Start(null, null);

            // 启动监控（可选）
            moniter.Start(server);

            // 结束服务器
            while (true)
            {
                if (Console.ReadKey().Key == ConsoleKey.Q)
                {
                    moniter.Stop();
                    server.Stop();
                    break;
                }
            }
        }

        /// <summary>
        /// 多服务器示例
        /// </summary>
        private static void MultiInsSample()
        {
            // 创建服务器
            var serverA = new Server("0.0.0.0", 5000);
            var serverB = new Server("0.0.0.0", 5001);
            var serverC = new Server("0.0.0.0", 5002);

            // 监听事件
            serverA.EventSessionClosed +=
                (session, reason) => Logger.Instance.InfoFormat("{0} disconnected from serverA by {1}", session.Id, reason);
            serverA.EventSessionEstablished += session => Logger.Instance.InfoFormat("{0} connected in ServerA", session.Id);

            serverB.EventSessionClosed +=
                (session, reason) => Logger.Instance.InfoFormat("{0} disconnected from serverB by {1}", session.Id, reason);
            serverB.EventSessionEstablished += session => Logger.Instance.InfoFormat("{0} connected in ServerB", session.Id);

            serverC.EventSessionClosed +=
                (session, reason) => Logger.Instance.InfoFormat("{0} disconnected from serverC by {1}", session.Id, reason);
            serverC.EventSessionEstablished += session => Logger.Instance.InfoFormat("{0} connected in ServerC", session.Id);

            // 启动服务器
            // 让serverA拥有自己独立的网络服务和逻辑服务，故传入参数为null
            serverA.Start(null, null);

            // 创建独立服务
            var logicService = new LogicService { Capacity = 10000, Period = 10 };
            var netService = new NetService { Capacity = 10000, Period = 10 };
            logicService.Start();
            netService.Start();

            // serverB和serverC共享服务
            serverB.Start(netService, logicService);
            serverC.Start(netService, logicService);

            // 结束服务器
            while (true)
            {
                if (Console.ReadKey().Key == ConsoleKey.Q)
                {
                    serverA.Stop();
                    serverB.Stop();
                    serverC.Stop();
                    netService.Stop();
                    logicService.Stop();

                    break;
                }
            }
        }

        /// <summary>
        /// 单服务器示例
        /// </summary>
        private static void SingleInsSample()
        {
            // 创建服务器
            var server = new Server("0.0.0.0", 5000);

            // 监听事件
            server.EventSessionClosed +=
                (session, reason) => Logger.Instance.InfoFormat("{0} disconnected by {1}", session.Id, reason);
            server.EventSessionEstablished += session => Logger.Instance.InfoFormat("{0} connected", session.Id);

            // 启动服务器
            // 注意：该服务器拥有自己独立的网络服务和逻辑服务，故传入参数为null
            server.Start(null, null);

            // 结束服务器
            while (true)
            {
                if (Console.ReadKey().Key == ConsoleKey.Q)
                {
                    server.Stop();
                    break;
                }
            }
        }
    }
}
