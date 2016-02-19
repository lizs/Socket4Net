using System;

namespace socket4net
{
    /// <summary>
    ///     空日志
    /// </summary>
    public class EmptyLogger : ILog
    {
        public void Debug(object message)
        {
        }

        public void Debug(string format, object arg0)
        {
        }

        public void Debug(string format, object arg0, object arg1)
        {
        }

        public void Debug(string format, object arg0, object arg1, object arg2)
        {
        }

        public void Error(object message)
        {
        }

        public void Error(string format, object arg0)
        {
        }

        public void Error(string format, object arg0, object arg1)
        {
        }

        public void Error(string format, object arg0, object arg1, object arg2)
        {
        }

        public void Fatal(object message)
        {
        }

        public void Fatal(string format, object arg0)
        {
        }

        public void Fatal(string format, object arg0, object arg1)
        {
        }

        public void Fatal(string format, object arg0, object arg1, object arg2)
        {
        }

        public void Info(object message)
        {
        }

        public void Info(string format, object arg0)
        {
        }

        public void Info(string format, object arg0, object arg1)
        {
        }

        public void Info(string format, object arg0, object arg1, object arg2)
        {
        }

        public void Warn(object message)
        {
        }

        public void Warn(string format, object arg0)
        {
        }

        public void Warn(string format, object arg0, object arg1)
        {
        }

        public void Warn(string format, object arg0, object arg1, object arg2)
        {
        }

        public void Exception(Exception e)
        {
        }

        public void Shutdown()
        {
        }
    }
}
