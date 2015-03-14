using Core.RPC;
using Core.Serialize;
using Proto;

namespace ChatS
{
    public class Chater : RpcHost
    {
        public Chater(RpcSession session) : base(session)
        {
        }

        protected override void RegisterRpcHandlers()
        {
            RequestsHandlers.Add(RpcRoute.GmCmd, HandleGmRequest);
            NotifyHandlers.Add(RpcRoute.Chat, HandleChatNotify);
        }

        private object HandleGmRequest(byte[] bytes)
        {
            var msg = Serializer.Deserialize<Message2Server>(bytes);

            return new Broadcast2Clients
            {
                From = Session.Id.ToString(),
                Message = "Gm command [" + msg.Message + "] Responsed"
            };
        }

        private bool HandleChatNotify(byte[] bytes)
        {
            var msg = Serializer.Deserialize<Message2Server>(bytes);
            var proto = new Broadcast2Clients
            {
                From = Session.Id.ToString(),
                Message = msg.Message
            };

            //RpcSession.NotifyAll(RpcRoute.Chat, proto);
            Session.Notify(RpcRoute.Chat, proto);
            return true;
        }
    }
}