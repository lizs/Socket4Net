using System;

namespace socket4net
{
    public class DefaultLogger : ILog
    {
        public void Debug(object message)
        {
            Console.WriteLine(message);
        }

        public void Debug(string format, object arg0)
        {
            Console.WriteLine(format, arg0);
        }

        public void Debug(string format, object arg0, object arg1)
        {
            Console.WriteLine(format, arg0, arg1);
        }

        public void Debug(string format, object arg0, object arg1, object arg2)
        {
            Console.WriteLine(format, arg0, arg1, arg2);
        }

        public void Error(object message)
        {
            Console.WriteLine(message);
        }

        public void Error(string format, object arg0)
        {
            Console.WriteLine(format, arg0);
        }

        public void Error(string format, object arg0, object arg1)
        {
            Console.WriteLine(format, arg0, arg1);
        }

        public void Error(string format, object arg0, object arg1, object arg2)
        {
            Console.WriteLine(format, arg0, arg1, arg2);
        }

        public void Fatal(object message)
        {
            Console.WriteLine(message);
        }

        public void Fatal(string format, object arg0)
        {
            Console.WriteLine(format, arg0);
        }

        public void Fatal(string format, object arg0, object arg1)
        {
            Console.WriteLine(format, arg0, arg1);
        }

        public void Fatal(string format, object arg0, object arg1, object arg2)
        {
            Console.WriteLine(format, arg0, arg1, arg2);
        }

        public void Info(object message)
        {
            Console.WriteLine(message);
        }

        public void Info(string format, object arg0)
        {
            Console.WriteLine(format, arg0);
        }

        public void Info(string format, object arg0, object arg1)
        {
            Console.WriteLine(format, arg0, arg1);
        }

        public void Info(string format, object arg0, object arg1, object arg2)
        {
            Console.WriteLine(format, arg0, arg1, arg2);
        }

        public void Warn(object message)
        {
            Console.WriteLine(message);
        }

        public void Warn(string format, object arg0)
        {
            Console.WriteLine(format, arg0);
        }

        public void Warn(string format, object arg0, object arg1)
        {
            Console.WriteLine(format, arg0, arg1);
        }

        public void Warn(string format, object arg0, object arg1, object arg2)
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
