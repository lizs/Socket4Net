
using Core.Net.TCP;
using Core.RPC;
using Core.Serialize;
using Core.Timer;
using Proto;

namespace ChatC
{
    public class Robot : Chater
    {
        public Robot(ITcpClient client) : base(client.Session as RpcSession)
        {
            _client = client;
        }

        private readonly ITcpClient _client;
        private readonly Timer _timer = new Timer("Robot", 1000, 500);

        public override void Dispose()
        {
            _timer.Arrived -= OnTimer;
            _timer.Dispose();

            _client.Shutdown();
            base.Dispose();
        }

        public override void Boot()
        {
            base.Boot();
//             _timer.Arrived += OnTimer;
//             _timer.Start();

            RequestGmCommand("in", true);
        }

        private void OnTimer(Timer timer)
        {
            // notify
            Session.Notify(RpcRoute.Chat, new Message2Server { Message = "i'm robot " + Session.Id });
            //RequestGmCommand("Request from Robot : " + Session.Id);
        }

        protected override bool HandleNotifyChat(byte[] bytes)
        {
            return true;
            var msg = Serializer.Deserialize<Broadcast2Clients>(bytes);
            //Console.WriteLine(msg.From + " : " + msg.Message);
        }
    }
}
