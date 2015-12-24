using System;
using Proto;
using socket4net;

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

            Obj.Create<Launcher>(new LauncherArg(null, new CustomLog.Log4Net("log4net.config", "ChatC")));
            Launcher.Instance.Start();

            // 启动客户端
            RunClient(ip, port);

            Launcher.Instance.Destroy();
        }

        private static void RunClient(string ip, ushort port)
        {
            // 创建客户端
            var client = Obj.Create<Client>(new ClientArg(null, ip, port){ AutoReconnectEnabled = true});
            
            // 监听事件
            client.EventSessionClosed +=
                (session, reason) => Logger.Instance.InfoFormat("{0} disconnected by {1}", session.Id, reason);
            client.EventSessionEstablished +=
                session => Logger.Instance.InfoFormat("{0} connected", session.Id);
            
            // 启动
            client.Start();

            // 结束
            var stop = false;
            while (!stop)
            {
                var msg = Console.ReadLine();
                if (string.IsNullOrEmpty(msg)) continue;

                switch (msg.ToUpper())
                {
                    case "QUIT":
                    case "EXIT":
                        {
                            client.Destroy();
                            stop = true;
                        }
                        break;

                    case "REQ":
                    {
                        //  请求服务器
                        client.RequestAsync(0, 0, (short) ECommand.Request, new RequestMsgProto {Message = msg}, 0, 0,
                            (b, bytes) =>
                            {
                                if (!b)
                                    Logger.Instance.Error("请求失败");
                                else
                                {
                                    var proto = PiSerializer.Deserialize<ResponseMsgProto>(bytes);
                                    Logger.Instance.Info(proto.Message);
                                }
                            });

                        break;
                    }

                    default:
                    {
                        //  请求服务器
                        client.Push(0, 0, (short) ECommand.Push, new PushMsgProto {Message = msg}, 0, 0);
                        break;
                    }
                }
            }
        }
    }
}
