using System;
using System.Collections.Generic;
using System.Text;
using socket4net;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace WServer
{
    class Program
    {
        static void Main(string[] args)
        {
            // 创建并启动Launcher
            Obj.New<Launcher>(LauncherArg.Default, true);

            // 创建并启动服务器
            var port = args.IsNullOrEmpty() ? 80 : int.Parse(args[0]);
            var wssv = new WebSocketServer($"ws://localhost:{port}");
            wssv.AddWebSocketService<ChatSession>("/chat");
            wssv.Start();
            if (wssv.IsListening)
            {
                Console.WriteLine("Listening on port {0}, and providing WebSocket services:", wssv.Port);
                foreach (var path in wssv.WebSocketServices.Paths)
                    Console.WriteLine("- {0}", path);
            }
            
            Console.WriteLine("Press any key to exit!");
            Console.ReadKey();

            wssv.Stop();

            // 销毁Launcher
            Launcher.Ins.Destroy();
        }
    }
}
