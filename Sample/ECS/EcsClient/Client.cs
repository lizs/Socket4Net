using System;
using socket4net;

namespace Sample
{
    public class Client : RpcClient<ChatSession>
    {
        public static Client Ins { get; private set; }

        public Client()
        {
            if (Ins != null)
                throw new Exception("Client already instantiated!");

            Ins = this;
        }
    }
}
