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
            //InvokeRepeating(Broadcast, (uint)Rand.Next(3 * 1000, 10 * 1000), (uint)Rand.Next(5 * 1000, 10 * 1000));
            InvokeRepeating(Request, (uint) Rand.Next(3*1000, 10*1000), 200);
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
            if (!Connected) return;

            CancelInvoke(Request);
            var ret = await RequestAsync(new DefaultDataProtocol
            {
                Ops = (short) EOps.Reqeust,
                Data = PbSerializer.Serialize(new RequestProto {Message = "I'm a request!"})
            });

            if (ret.Key)
            {
                Request();
            }
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
            var cnt = args.IsNullOrEmpty() ? 100 : int.Parse(args[0]);
            for (var i = 0; i < cnt; i++)
            {
                clients.Create<MyClient>(new WebsocketClientArg(clients, Uid.Create(), "ws://localhost:9527/chat"), true);
            }

            Console.ReadLine();
            clients.Destroy();

            // 销毁Launcher
            Launcher.Ins.Destroy();
        }
    }
}