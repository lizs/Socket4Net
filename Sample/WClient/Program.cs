using System;
using System.Threading.Tasks;
using Proto;
using socket4net;
using socket4net.objects;
using WebSocketSharp;

namespace WClient
{
    public class MyClient : WebsocketClient
    {
        public MyClient(WebSocket ws) : base(ws)
        {
        }

        public override async Task<bool> OnPush(IDataProtocol ps)
        {
            var more = ps as DefaultDataProtocol;
            switch ((EOps)more.Ops)
            {
                case EOps.Push:
                    {
                        var proto = PiSerializer.Deserialize<PushProto>(more.Data);
                        return true;
                    }

                default:
                    return false;
            }
        }

        public override Task<RpcResult> OnRequest(IDataProtocol rq)
        {
            return base.OnRequest(rq);
        }
    }

    public class MyClientWrapper : ObjWrapper<MyClient>
    {
        protected override void OnStart()
        {
            base.OnStart();
            Object.ConnectAsync();
            InvokeRepeating(Broadcast, (uint)Rand.Next(3 * 1000, 10 * 1000), (uint)Rand.Next(5 * 1000, 10 * 1000));
            InvokeRepeating(Request, (uint)Rand.Next(3 * 1000, 10 * 1000), 200);
        }

        private async void Broadcast()
        {
            await Object.Push(new DefaultDataProtocol
            {
                Ops = (short)EOps.Push,
                Data = PiSerializer.Serialize(new PushProto { Message = "Hello socket4net!" })
            });
        }
        
        private async void Request()
        {
            await Object.RequestAsync(new DefaultDataProtocol
            {
                Ops = (short)EOps.Reqeust,
                Data = PiSerializer.Serialize(new RequestProto() { Message = "I'm a request!" })
            });
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Object.Close();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // 创建并启动Launcher
            Obj.New<Launcher>(LauncherArg.Default, true);

            // 创建并启动客户端
            var clients = Obj.New<Mgr<MyClientWrapper>>(ObjArg.Empty);
            var cnt = args.IsNullOrEmpty() ? 300 : int.Parse(args[0]);
            for (var i = 0; i < cnt; i++)
            {
                var ws = new WebSocket("ws://localhost/chat");
                var client = new MyClient(ws);
                clients.Create<MyClientWrapper>(new ObjWrapperArg<MyClient>(clients, client), true);
            }

            Console.ReadLine();
            clients.Destroy();

            // 销毁Launcher
            Launcher.Ins.Destroy();
        }
    }
}
