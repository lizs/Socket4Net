using System;
using Proto;
using socket4net;

namespace Sample
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var ip = "127.0.0.1";
            ushort port = 843;

            if (args.Length > 0)
            {
                ip = args[0];
                if (args.Length > 1)
                    port = ushort.Parse(args[1]);
            }

            Obj.Create<Launcher>(new LauncherArg(new CustomLog.Log4Net("log4net.config", "Sample")), true);

            // 启动客户端
            RunClient(ip, port);

            Launcher.Ins.Destroy();
        }

        private static void RunClient(string ip, ushort port)
        {
            // 创建并启动客户端
            var client = Obj.Create<Client>(new ClientArg(null, ip, port), true);

            // 结束
            var stopped = false;
            while (!stopped)
            {
                var msg = Console.ReadLine();
                if (string.IsNullOrEmpty(msg)) continue;

                switch (msg.ToUpper())
                {
                    case "QUIT":
                    case "EXIT":
                    {
                        client.LogicService.Perform(client.Destroy);
                        stopped = true;
                    }
                        break;

                    case "REQ":
                    {
                        //  请求服务器
                        client.RequestAsync(0, 0, (short) ECommand.Request, new RequestMsgProto {Message = msg}, 0, 0,
                            (b, bytes) =>
                            {
                                if (!b)
                                    Logger.Ins.Error("请求失败");
                                else
                                {
                                    var proto = PiSerializer.Deserialize<ResponseMsgProto>(bytes);
                                    Logger.Ins.Info(proto.Message);
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
