using System;
using System.Text;
using System.Threading;
using Core.Log;
using Core.Net.TCP;
using Core.Service;

namespace ChatC
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var ip = "127.0.0.1";
            ushort port = 5000;

            if (args.Length > 0)
            {
                ip = args[0];
                if (args.Length > 1)
                    port = ushort.Parse(args[1]);
            }

            // 初始Logger
            Logger.Instance = new CustomLog.Log4Net();

            ClientSample(ip, port);
            //UnityClientSample(ip, port);
        }

        private static void ClientSample(string ip, ushort port)
        {
            // 创建客户端
            var client = new Client<ChatSession>(ip, port);

            // 监听事件
            client.EventSessionClosed +=
                (session, reason) => Logger.Instance.InfoFormat("{0} disconnected by {1}", session.Id, reason);
            client.EventSessionEstablished +=
                session => Logger.Instance.InfoFormat("{0} connected", session.Id);

            // 启动客户端
            // 注意：该客户端拥有自己独立的网络服务和逻辑服务，故传入参数为null
            client.Start(null, null);

            // 结束服务器
            while (true)
            {
                if (!client.Connected)
                    Thread.Sleep(1);

                var cmd = Console.ReadLine();
                switch (cmd.ToUpper())
                {
                    case "QUIT":
                    case "EXIT":
                        {
                            client.Stop();
                        }
                        break;

                    case "REQUEST":
                        {
                            client.Session.RequestCommand(
                                "This is a RPC response, indicate that server response your request success when you receive this message!");
                        }
                        continue;

                    default:
                        {
                            client.Session.PushMessage(cmd);
                        }
                        continue;
                }
            }
        }

        private static void UnityClientSample(string ip, ushort port)
        {
            // 创建客户端
            var client = new UnityClient<ChatSession>(ip, port);

            // 监听事件
            client.EventSessionClosed +=
                (session, reason) => Logger.Instance.InfoFormat("{0} disconnected by {1}", session.Id, reason);
            client.EventSessionEstablished +=
                session => Logger.Instance.InfoFormat("{0} connected", session.Id);

            // 启动客户端
            // 注意：该客户端拥有自己独立的网络服务和逻辑服务，故传入参数为null
            // 超级警告：由于UnityClient直接使用Unity的逻辑线程作为自己的逻辑服务
            // 线程，所以需要在某个MonoBehaviour的Update或FixedUpdate中调用
            // client.LogicService.Update(delta)来驱动逻辑服务
            client.Start(null, null);

            // 结束客户端
            var stop = false;
            while (!stop)
            {
                Thread.Sleep(1);

                // 使用主线程来模拟unity逻辑线程
                (client.LogicService as LogicService4Unity).Update(.0f);

                ThreadPool.QueueUserWorkItem(state =>
                {
                    while (!stop)
                    {
                        var cmd = Console.ReadLine();
                        switch (cmd.ToUpper())
                        {
                            case "QUIT":
                            case "EXIT":
                                {
                                    client.PerformInLogic(() =>
                                    {
                                        client.Stop();
                                        stop = true;
                                    });
                                }
                                break;

                            case "REQUEST":
                                {
                                    client.PerformInLogic(() => client.Session.RequestCommand(
                                        "This is a rpc request, specified server response success when you receive this message!"));
                                }
                                break;

                            default:
                                {
                                    client.PerformInLogic(() => client.Session.PushMessage(cmd));
                                }
                                break;
                        }
                    }
                });
            }
        }
    }
}
