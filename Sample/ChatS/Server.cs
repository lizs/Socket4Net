using System;
using socket4net;

namespace ChatS
{
    public class Server : Server<ChatSession>
    {
        public static Server Instance { get; private set; }

        public Server()
        {
            if(Instance != null)
                throw new Exception("Server already instantiated!");

            Instance = this;
        }
    }
}
