using System;
using node;

namespace Sample
{
    public class ChatNodesMgr : NodesMgr
    {
        public ChatNodesMgr()
        {
            if (_ins != null) throw new Exception("ChatNodesMgr already instantiated!");
            _ins = this;
        }

        private static ChatNodesMgr _ins;
        public static ChatNodesMgr Ins { get { return _ins; } }

        protected override Type MapSession(string type)
        {
            switch (type.ToUpper())
            {
                case "CHAT":
                    return typeof (ChatSession);
            }

            throw new ArgumentException("type");
        }
    }
}
