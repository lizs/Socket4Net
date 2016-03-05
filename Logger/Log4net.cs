#region MIT
//  /*The MIT License (MIT)
// 
//  Copyright 2016 lizs lizs4ever@163.com
//  
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//  
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
//  
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
//   * */
#endregion

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
