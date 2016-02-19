using System;
using socket4net;

namespace ChatS
{
    public class Server : Server<ChatSession>
    {
        public static Server Ins { get; private set; }

        public Server()
        {
            if(Ins != null)
                throw new Exception("Server already instantiated!");

            Ins = this;
        }
    }
}
