using System;
using ecs;
using node;

namespace Sample
{
    public class ServerNodesMgr : NodesMgr
    {
        public ServerNodesMgr()
        {
            if (_ins != null) throw new Exception("ServerNodesMgr already instantiated!");
            _ins = this;
        }

        private static ServerNodesMgr _ins;
        public static ServerNodesMgr Ins { get { return _ins; } }

        protected override Type MapSession(string type)
        {
            switch (type.ToUpper())
            {
                case "SAMPLE":
                    return typeof (DispatchableSession);
            }

            throw new ArgumentException("type");
        }
    }
}
