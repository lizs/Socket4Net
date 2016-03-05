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
using System.Linq;
using CustomLog;
using ecs;
using node;
using socket4net;

namespace Sample
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // App.Config
            var launcherCfg = LauncherConfig.LoadAs<ClientConfig>("Client.exe.config");

            // 创建并启动Launcher
            var arg = new LauncherArg<ClientConfig>(launcherCfg, new Log4Net(launcherCfg.LogConfig.File, "Client"));
            Obj.New<MyLauncher>(arg, true);

            Test();

            // 销毁Launcher
            Launcher.Ins.Destroy();
        }

        private static ecs.Player Player
        {
            get { return PlayerMgr.Ins.FirstOrDefault(); }
        }

        private static SampleComponent Component
        {
            get { return Player.GetComponent<SampleComponent>(); }
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

                    case "ECHO":
                    {
                        Component.Echo(msg);
                        break;
                    }

                    case "ADD":
                    {
                        Component.Add();
                        break;
                    }

                    case "DEL":
                    {
                        Component.Del();
                        break;
                    }

                    default:
                    {
                        Component.Broadcast(msg);
                        break;
                    }
                }
            }
        }
    }
}
