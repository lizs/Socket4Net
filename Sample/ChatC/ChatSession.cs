using System;
using System.Threading.Tasks;
using Core.Log;
using Core.RPC;
using Core.Serialize;
using Proto;

namespace ChatC
{
    /// <summary>
    /// 聊天者客户端实现
    /// </summary>
    public class ChatSession : RpcSession
    {
        private DateTime _rqTime;
        
        /// <summary>
        /// 发起RpcRoute.GmCmd请求
        /// </summary>
        /// <param name="command"></param>
        /// <param name="autorq"></param>
        public async void RequestCommand(string command)
        {
            _rqTime = DateTime.Now;

            // 请求服务器
            var ret = await RequestAsync((short)RpcRoute.GmCmd, new Message2Server { Message = command });

            // 处理服务器响应（异步回调）
            if (ret.Item1)
            {
                var delay = (DateTime.Now - _rqTime).Ticks / TimeSpan.TicksPerMillisecond;
                var responseMsg = Serializer.Deserialize<Broadcast2Clients>(ret.Item2);
                Logger.Instance.InfoFormat("responseMsg.From +  : {0} with delay {1}ms", responseMsg.Message, delay);
            }
            else
                Logger.Instance.Info("Server response : false!");
        }

        /// <summary>
        /// 发起RpcRoute.Chat通知
        /// </summary>
        /// <param name="msg"></param>
        public void PushMessage(string msg)
        {
            Push((short)RpcRoute.Chat, new Message2Server() { Message = msg });
        }

        public async override Task<Tuple<bool, byte[]>> HandleRequest(short route, byte[] param)
        {
            switch ((RpcRoute)route)
            {
                default:
                    return null;
            }
        }

        public async override Task<bool> HandlePush(short route, byte[] param)
        {
            switch ((RpcRoute)route)
            {
                case RpcRoute.Chat:
                {
                    var msg = Serializer.Deserialize<Broadcast2Clients>(param);
                    Logger.Instance.Info(msg.From + " : " + msg.Message);
                    return true;
                }

                default:
                    return false;
            }
        }
    }
}