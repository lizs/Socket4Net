﻿using System;
using socket4net;
using WebSocketSharp.Server;

namespace WServer
{
    class Program
    {
        static void Main(string[] args)
        {
            // 创建并启动Launcher
            Obj.Create<Launcher>(LauncherArg.Default, true);

            // 创建并启动服务器
            var port = args.IsNullOrEmpty() ? 80 : int.Parse(args[0]);
            var wssv = new WebSocketServer($"ws://localhost:{port}");
            wssv.AddWebSocketService<ChatSession>("/chat");
            wssv.Start();
            if (wssv.IsListening)
            {
                Console.WriteLine($"Listening on port {wssv.Port}, and providing WebSocket services:");
                foreach (var path in wssv.WebSocketServices.Paths)
                    Console.WriteLine($"- {path}");
            }
            
            Console.WriteLine("Press any key to exit!");
            Console.ReadKey();

            wssv.Stop();

            // 销毁Launcher
            Launcher.Ins.Destroy();
        }
    }
}
