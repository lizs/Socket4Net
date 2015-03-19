using Core.RPC;
using Core.Serialize;
using Proto;

namespace ChatS
{
    public class ChatSession : RpcSession
    {
        public override object HandleRequest(short route, byte[] param)
        {
            switch ((RpcRoute)route)
            {
                case RpcRoute.GmCmd:
                    {
                        var msg = Serializer.Deserialize<Message2Server>(param);

                        // 对于Request请求，回以ProtoBuf实例
                        // 该实例最终被对端请求者接收
                        return new Broadcast2Clients
                        {
                            From = Id.ToString(),
                            Message = "Gm command [" + msg.Message + "] Responsed"
                        };
                    }

                default:
                    return null;
            }
        }

        public override bool HandlePush(short route, byte[] param)
        {

            switch ((RpcRoute)route)
            {
                case RpcRoute.Chat:
                    {
                        var msg = Serializer.Deserialize<Message2Server>(param);
                        var proto = new Broadcast2Clients
                        {
                            From = Id.ToString(),
                            Message = msg.Message
                        };

                        // 广播给其他参与聊天者
                        PushAll((short)RpcRoute.Chat, proto);

                        // 或者只通知自己
                        //Session.Push(RpcRoute.Chat, proto);

                        return true;
                    }

                default:
                    return false;
            }
        }
    }
}