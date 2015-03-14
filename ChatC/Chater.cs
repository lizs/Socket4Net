using System;
using Core.RPC;
using Core.Serialize;
using Proto;

namespace ChatC
{
    public class Chater : RpcHost
    {
        protected static readonly log4net.ILog Log =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Chater(RpcSession session) : base(session)
        {
        }

        protected override void RegisterRpcHandlers()
        {
            NotifyHandlers.Add(RpcRoute.Chat, HandleNotifyChat);
        }

        protected virtual bool HandleNotifyChat(byte[] bytes)
        {
            var msg = Serializer.Deserialize<Broadcast2Clients>(bytes);
            //Console.WriteLine(msg.From + " : " + msg.Message);

            return true;
        }

        private DateTime _rqTime;

        public void RequestGmCommand(string command, bool autorq)
        {
            _rqTime = DateTime.Now;

            Session.Request(RpcRoute.GmCmd, new Message2Server { Message = command }, (success, bytes) =>
            {
                if (success)
                {
                    var delay = (DateTime.Now - _rqTime).Ticks/TimeSpan.TicksPerMillisecond;

                    var responseMsg = Serializer.Deserialize<Broadcast2Clients>(bytes);
                    if (autorq)
                        RequestGmCommand("Auto reply", true);
                    else
                    {
                        Console.WriteLine("responseMsg.From +  : {0} with delay {1}ms" ,responseMsg.Message, delay);
                    }
                }
                else
                    Console.WriteLine("Server response : false!");
            });
        }
    }
}