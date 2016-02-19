
using System;
using System.Diagnostics;
using System.IO;
using log4net;
using log4net.Config;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace CustomLog
{
    public class Log4Net : socket4net.ILog
    {
        private readonly ILog _log;
        public Log4Net(string cfgPath, string logName)
        {
            GlobalContext.Properties["LogName"] = logName;
            GlobalContext.Properties["ProcessId"] = Process.GetCurrentProcess().Id;

            var fi = new FileInfo(cfgPath);
            XmlConfigurator.Configure(fi);
            _log = LogManager.GetLogger(typeof(Log4Net));
        }

        public void Debug(object message)
        {
            _log.Debug(message);
        }

        public void Debug(string format, object arg0)
        {
            _log.DebugFormat(format, arg0);
        }

        public void Debug(string format, object arg0, object arg1)
        {
            _log.DebugFormat(format, arg0, arg1);
        }

        public void Debug(string format, object arg0, object arg1, object arg2)
        {
            _log.DebugFormat(format, arg0, arg1, arg2);
        }

        public void Error(object message)
        {
            _log.Error(message);
        }

        public void Error(string format, object arg0)
        {
            _log.ErrorFormat(format, arg0);
        }

        public void Error(string format, object arg0, object arg1)
        {
            _log.ErrorFormat(format, arg0, arg1);
        }

        public void Error(string format, object arg0, object arg1, object arg2)
        {
            _log.ErrorFormat(format, arg0, arg1, arg2);
        }

        public void Fatal(object message)
        {
            _log.Fatal(message);
        }

        public void Fatal(string format, object arg0)
        {
            _log.FatalFormat(format, arg0);
        }

        public void Fatal(string format, object arg0, object arg1)
        {
            _log.FatalFormat(format, arg0, arg1);
        }

        public void Fatal(string format, object arg0, object arg1, object arg2)
        {
            _log.FatalFormat(format, arg0, arg1, arg2);
        }

        public void Info(object message)
        {
            _log.Info(message);
        }

        public void Info(string format, object arg0)
        {
            _log.InfoFormat(format, arg0);
        }

        public void Info(string format, object arg0, object arg1)
        {
            _log.InfoFormat(format, arg0, arg1);
        }

        public void Info(string format, object arg0, object arg1, object arg2)
        {
            _log.InfoFormat(format, arg0, arg1, arg2);
        }

        public void Warn(object message)
        {
            _log.Warn(message);
        }

        public void Warn(string format, object arg0)
        {
            _log.WarnFormat(format, arg0);
        }

        public void Warn(string format, object arg0, object arg1)
        {
            _log.WarnFormat(format, arg0, arg1);
        }

        public void Warn(string format, object arg0, object arg1, object arg2)
        {
            _log.WarnFormat(format, arg0, arg1, arg2);
        }

        public void Exception(Exception e)
        {
            _log.FatalFormat("{0}:{1}", e.InnerException != null ? e.InnerException.Message : e.Message, e.StackTrace);
        }

        public void Shutdown()
        {
            log4net.LogManager.Shutdown();
        }
    }
}
