using Core.RPC;
using Core.Serialize;
using Proto;

namespace ChatS
{
    /// <summary>
    /// 聊天者，与每个对端一一对应
    /// 具体实现了RpcHost
    /// </summary>
    public class Chater : RpcHost
    {
        public Chater(RpcSession session) : base(session)
        {
        }

        /// <summary>
        /// 重写基类方法
        /// 明确自己处理哪些请求
        /// </summary>
        protected override void RegisterRpcHandlers()
        {
            RegisterNotifyHandler(RpcRoute.Chat, HandleChatNotify);
            RegisterRequestHandler(RpcRoute.GmCmd, HandleGmRequest);
        }

        /// <summary>
        /// 处理RpcRoute.GmCmd
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private object HandleGmRequest(byte[] bytes)
        {
            var msg = Serializer.Deserialize<Message2Server>(bytes);

            // 对于Request请求，回以ProtoBuf实例
            // 该实例最终被对端请求者接收
            return new Broadcast2Clients
            {
                From = Session.Id.ToString(),
                Message = "Gm command [" + msg.Message + "] Responsed"
            };
        }

        /// <summary>
        /// 处理RpcRoute.Chat
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private bool HandleChatNotify(byte[] bytes)
        {
            var msg = Serializer.Deserialize<Message2Server>(bytes);
            var proto = new Broadcast2Clients
            {
                From = Session.Id.ToString(),
                Message = msg.Message
            };

            // 广播给其他参与聊天者
            Session.NotifyAll(RpcRoute.Chat, proto);

            // 或者只通知自己
            //Session.Notify(RpcRoute.Chat, proto);

            return true;
        }
    }
}