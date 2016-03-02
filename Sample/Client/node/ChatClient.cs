using System;
using ecs;
using node;
namespace Sample
{
    public class ChatClient : ClientNode<ChatSession>
    {
        public static ChatClient Ins { get; private set; }

        public ChatClient()
        {
            if(Ins != null)
                throw new Exception("ChatClient already instantiated!");

            Ins = this;
        }
    }
}
