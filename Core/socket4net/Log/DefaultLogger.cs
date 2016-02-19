using System;

namespace socket4net
{
    public class DefaultLogger : ILog
    {
        public void Debug(object message)
        {
            Console.WriteLine(message);
        }

        public void DebugFormat(string format, object arg0)
        {
            Console.WriteLine(format, arg0);
        }

        public void DebugFormat(string format, object arg0, object arg1)
        {
            Console.WriteLine(format, arg0, arg1);
        }

        public void DebugFormat(string format, object arg0, object arg1, object arg2)
        {
            Console.WriteLine(format, arg0, arg1, arg2);
        }

        public void Error(object message)
        {
            Console.WriteLine(message);
        }

        public void ErrorFormat(string format, object arg0)
        {
            Console.WriteLine(format, arg0);
        }

        public void ErrorFormat(string format, object arg0, object arg1)
        {
            Console.WriteLine(format, arg0, arg1);
        }

        public void ErrorFormat(string format, object arg0, object arg1, object arg2)
        {
            Console.WriteLine(format, arg0, arg1, arg2);
        }

        public void Fatal(object message)
        {
            Console.WriteLine(message);
        }

        public void FatalFormat(string format, object arg0)
        {
            Console.WriteLine(format, arg0);
        }

        public void FatalFormat(string format, object arg0, object arg1)
        {
            Console.WriteLine(format, arg0, arg1);
        }

        public void FatalFormat(string format, object arg0, object arg1, object arg2)
        {
            Console.WriteLine(format, arg0, arg1, arg2);
        }

        public void Info(object message)
        {
            Console.WriteLine(message);
        }

        public void InfoFormat(string format, object arg0)
        {
            Console.WriteLine(format, arg0);
        }

        public void InfoFormat(string format, object arg0, object arg1)
        {
            Console.WriteLine(format, arg0, arg1);
        }

        public void InfoFormat(string format, object arg0, object arg1, object arg2)
        {
            Console.WriteLine(format, arg0, arg1, arg2);
        }

        public void Warn(object message)
        {
            Console.WriteLine(message);
        }

        public void WarnFormat(string format, object arg0)
        {
            Console.WriteLine(format, arg0);
        }

        public void WarnFormat(string format, object arg0, object arg1)
        {
            Console.WriteLine(format, arg0, arg1);
        }

        public void WarnFormat(string format, object arg0, object arg1, object arg2)
        {
            Console.WriteLine(format, arg0, arg1, arg2);
        }

        public void Exception(Exception e)
        {
            Console.WriteLine("{0}:{1}", e.InnerException != null ? e.InnerException.Message : e.Message, e.StackTrace);
        }

        public void Shutdown()
        {
        }
    }
}
