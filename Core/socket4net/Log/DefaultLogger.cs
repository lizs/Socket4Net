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

namespace socket4net
{
    /// <summary>
    ///     socket4net's default logger
    /// </summary>
    public class DefaultLogger : ILog
    {
        public void Debug(object message)
        {
            Console.WriteLine(message);
        }

        public void Debug(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        public void Error(object message)
        {
            Console.WriteLine(message);
        }

        public void Error(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        public void Fatal(object message)
        {
            Console.WriteLine(message);
        }

        public void Fatal(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        public void Info(object message)
        {
            Console.WriteLine(message);
        }

        public void Info(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }
       
        public void Warn(object message)
        {
            Console.WriteLine(message);
        }

        public void Warn(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        public void Exception(string msg, Exception e)
        {
            Console.WriteLine("{0}:{1}:{2}", msg, e.InnerException != null ? e.InnerException.Message : e.Message,
                e.StackTrace);
        }

        public void Shutdown()
        {
        }
    }
}
