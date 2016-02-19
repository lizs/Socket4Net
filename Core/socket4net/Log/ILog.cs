using System;

namespace socket4net
{
    public interface ILog
    {
        void Debug(object message);
        void Debug(string format, object arg0);
        void Debug(string format, object arg0, object arg1);
        void Debug(string format, object arg0, object arg1, object arg2);
        void Error(object message);
        void Error(string format, object arg0);
        void Error(string format, object arg0, object arg1);
        void Error(string format, object arg0, object arg1, object arg2);
        void Fatal(object message);
        void Fatal(string format, object arg0);
        void Fatal(string format, object arg0, object arg1);
        void Fatal(string format, object arg0, object arg1, object arg2);
        void Info(object message);
        void Info(string format, object arg0);
        void Info(string format, object arg0, object arg1);
        void Info(string format, object arg0, object arg1, object arg2);
        void Warn(object message);
        void Warn(string format, object arg0);
        void Warn(string format, object arg0, object arg1);
        void Warn(string format, object arg0, object arg1, object arg2);
        void Exception(Exception e);

        void Shutdown();
    }
}
