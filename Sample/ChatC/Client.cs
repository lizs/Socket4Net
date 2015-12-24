using System;
using socket4net;

namespace ChatC
{
    public class Client : RpcClient<ChatSession>
    {
        public static Client Instance { get; private set; }

        public Client()
        {
            if (Instance != null)
                throw new Exception("Client already instantiated!");

            Instance = this;
        }
    }
}
