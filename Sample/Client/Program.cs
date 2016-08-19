#region MIT
//  /*The MIT License (MIT)
// 
//  Copyright 2016 lizs lizs4ever@163.com
//  
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//  
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
//  
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
//   * */
#endregion
using System;
using System.Collections.Generic;
using Proto;
using socket4net;

namespace Sample
{
    internal class Program
    {
        private static Client<ChatSession> _chatClient; 

        private static void Main(string[] args)
        {
            // 创建并启动Launcher
            Obj.New<Launcher>(LauncherArg.Default, true);

            // 创建并启动客户端
            var clients = new List<Obj>();
            var cnt = args.IsNullOrEmpty() ? 300 : int.Parse(args[0]);
            for (int i = 0; i < cnt; i++)
            {
                clients.Add(Obj.New<Client<ChatSession>>(new ClientArg(null, "127.0.0.1", 9527, true), true));
            }

            Console.ReadLine();
            foreach (var client in clients)
            {
                client.Destroy();
            }

            // 销毁Launcher
            Launcher.Ins.Destroy();
        }

        private static void Test()
        {
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
                        stopped = true;
                        break;
                    }

                    case "REQ":
                    {
                        _chatClient.Session.RequestAsync(new DefaultDataProtocol
                        {
                            Ops = (short) EOps.Reqeust,
                            Data = PiSerializer.Serialize(new RequestProto {Message = msg})
                        }, (b, bytes) =>
                        {
                            if (!b)
                                Logger.Ins.Error("Request failed!");
                            else
                            {
                                var proto = PiSerializer.Deserialize<ResponseProto>(bytes);
                                Logger.Ins.Info(proto.Message);
                            }
                        });
                        break;
                    }

                    default:
                    {
                        _chatClient.Session.Push(new DefaultDataProtocol
                        {
                            Ops = (short) EOps.Push,
                            Data = PiSerializer.Serialize(new PushProto {Message = msg})
                        });
                        break;
                    }
                }
            }
        }
    }
}
