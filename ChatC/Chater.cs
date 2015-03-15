using System;
using Core.Log;
using Core.RPC;
using Core.Serialize;
using Proto;

namespace ChatC
{
    /// <summary>
    /// 聊天者客户端实现
    /// </summary>
    public class Chater : RpcHost
    {
        public Chater(RpcSession session)
            : base(session)
        {
        }

        /// <summary>
        /// 明确自己能够处理的包
        /// </summary>
        protected override void RegisterRpcHandlers()
        {
            NotifyHandlers.Add(RpcRoute.Chat, HandleNotifyChat);
        }

        /// <summary>
        /// 处理聊天消息通知
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private bool HandleNotifyChat(byte[] bytes)
        {
            var msg = Serializer.Deserialize<Broadcast2Clients>(bytes);
            Logger.Instance.Info(msg.From + " : " + msg.Message);
            return true;
        }

        private DateTime _rqTime;
        
        /// <summary>
        /// 发起RpcRoute.GmCmd请求
        /// </summary>
        /// <param name="command"></param>
        /// <param name="autorq"></param>
        public void RequestCommand(string command)
        {
            _rqTime = DateTime.Now;

            // 请求服务器
            Session.Request(RpcRoute.GmCmd, new Message2Server { Message = command },
                // 处理服务器响应（异步回调）
                (success, bytes) =>
                {
                    if (success)
                    {
                        var delay = (DateTime.Now - _rqTime).Ticks / TimeSpan.TicksPerMillisecond;

                        var responseMsg = Serializer.Deserialize<Broadcast2Clients>(bytes);

                        Logger.Instance.InfoFormat("responseMsg.From +  : {0} with delay {1}ms", responseMsg.Message, delay);
                    }
                    else
                        Logger.Instance.Info("Server response : false!");
                });
        }

        /// <summary>
        /// 发起RpcRoute.Chat通知
        /// </summary>
        /// <param name="msg"></param>
        public void NotifyMessage(string msg)
        {
            Session.Notify(RpcRoute.Chat, new Message2Server() { Message = msg });
        }
    }
}