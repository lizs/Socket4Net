
using System;

namespace Core.Log
{
    public class DefaultLogger : ILog
    {
        public void Debug(object message)
        {
            Console.WriteLine("Debug : " + message);
        }

        public void DebugFormat(string format, object arg0)
        {
            Console.WriteLine("Debug : " + format, arg0);
        }

        public void DebugFormat(string format, object arg0, object arg1)
        {
            Console.WriteLine("Debug : " + format, arg0, arg1);
        }

        public void DebugFormat(string format, object arg0, object arg1, object arg2)
        {
            Console.WriteLine("Debug : " + format, arg0, arg1, arg2);
        }

        public void Error(object message)
        {
            Console.WriteLine("Error : " + message);
        }

        public void ErrorFormat(string format, object arg0)
        {
            Console.WriteLine("Error : " + format, arg0);
        }

        public void ErrorFormat(string format, object arg0, object arg1)
        {
            Console.WriteLine("Error : " + format, arg0, arg1);
        }

        public void ErrorFormat(string format, object arg0, object arg1, object arg2)
        {
            Console.WriteLine("Error : " + format, arg0, arg1, arg2);
        }

        public void Fatal(object message)
        {
            Console.WriteLine("Fatal : " + message);
        }

        public void FatalFormat(string format, object arg0)
        {
            Console.WriteLine("Fatal : " + format, arg0);
        }

        public void FatalFormat(string format, object arg0, object arg1)
        {
            Console.WriteLine("Fatal : " + format, arg0, arg1);
        }

        public void FatalFormat(string format, object arg0, object arg1, object arg2)
        {
            Console.WriteLine("Fatal : " + format, arg0, arg1, arg2);
        }

        public void Info(object message)
        {
            Console.WriteLine("Info : " + message);
        }

        public void InfoFormat(string format, object arg0)
        {
            Console.WriteLine("Info : " + format, arg0);
        }

        public void InfoFormat(string format, object arg0, object arg1)
        {
            Console.WriteLine("Info : " + format, arg0, arg1);
        }

        public void InfoFormat(string format, object arg0, object arg1, object arg2)
        {
            Console.WriteLine("Info : " + format, arg0, arg1, arg2);
        }

        public void Warn(object message)
        {
            Console.WriteLine("Warn : " + message);
        }

        public void WarnFormat(string format, object arg0)
        {
            Console.WriteLine("Warn : " + format, arg0);
        }

        public void WarnFormat(string format, object arg0, object arg1)
        {
            Console.WriteLine("Warn : " + format, arg0, arg1);
        }

        public void WarnFormat(string format, object arg0, object arg1, object arg2)
        {
            Console.WriteLine("Warn : " + format, arg0, arg1, arg2);
        }
    }
}
