using System;
using System.Threading.Tasks;
using Proto;
using socket4net;

namespace WClient
{
    public class MyClient : WebsocketClient
    {
        public override Task<bool> OnPush(IDataProtocol ps)
        {
            return Task.FromResult(true);
        }

        protected override void OnStart()
        {
            base.OnStart();
            ConnectAsync();
        }

        protected override void OnOpen()
        {
            base.OnOpen();
            InvokeRepeating(Request, (uint)Rand.Next(3 * 1000, 3 * 1000), (uint)Rand.Next(1 * 1000, 5 * 1000));
            //InvokeRepeating(Broadcast, (uint) Rand.Next(3*1000, 10*1000), (uint) Rand.Next(1*1000, 1*1000));
        }

        private async void Broadcast()
        {
            if (!Connected) return;

            await Push(new DefaultDataProtocol
            {
                Ops = (short) EOps.Push,
                Data = PbSerializer.Serialize(new PushProto {Message = "Hello socket4net!"})
            });
        }

        private async void Request()
        {
            var ret = await RequestAsync(new DefaultDataProtocol
            {
                Ops = (short) EOps.Reqeust,
                Data = PbSerializer.Serialize(new RequestProto {Message = "I'm a request!"})
            });

//            if (ret.Key)
//            {
//                InvokeRepeating(Request, (uint)Rand.Next(3 * 1000, 3 * 1000), (uint)Rand.Next(1 * 1000, 5 * 1000));
//                Request();
//            }
        }
    }


    internal class Program
    {
        private static void Main(string[] args)
        {
            // 创建并启动Launcher
            Obj.Create<Launcher>(LauncherArg.Default, true);

            // 创建并启动客户端
            var clients = Obj.Create<Mgr<MyClient>>(ObjArg.Empty, true);
            var host = args.IsNullOrEmpty() ? "localhost" : args[0];
            var port = args.Length > 1 ? int.Parse(args[1]) : 9527;
            var cnt = args.Length > 2 ? int.Parse(args[2]) : 100;
            for (var i = 0; i < cnt; i++)
            {
                clients.Create<MyClient>(new WebsocketClientArg(clients, Uid.Create(), $"ws://{host}:{port}/chat"), true);
            }

            Console.ReadLine();
            clients.Destroy();

            // 销毁Launcher
            Launcher.Ins.Destroy();
        }
    }
}