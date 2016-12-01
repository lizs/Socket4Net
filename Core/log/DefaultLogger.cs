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
using WebSocketSharp;

namespace socket4net
{
    /// <summary>
    ///     socket4net's default logger
    ///     with websocket-sharp's logger implementation
    /// </summary>
    public class DefaultLogger : ILog
    {
        /// <summary>
        ///     Log as debug level
        /// </summary>
        public void Debug(object message)
        {
            Console.WriteLine(message);
        }

        /// <summary>
        ///     Log as debug level
        /// </summary>
        public void Debug(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        /// <summary>
        ///     Log as error level
        /// </summary>
        public void Error(object message)
        {
            Console.WriteLine(message);
        }

        /// <summary>
        ///     Log as error level
        /// </summary>
        public void Error(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        /// <summary>
        ///     Log as fatal level
        /// </summary>
        public void Fatal(object message)
        {
            Console.WriteLine(message);
        }

        /// <summary>
        ///     Log as fatal level
        /// </summary>
        public void Fatal(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        /// <summary>
        ///     Log as information level
        /// </summary>
        public void Info(object message)
        {
            Console.WriteLine(message);
        }

        /// <summary>
        ///     Log as information level
        /// </summary>
        public void Info(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        /// <summary>
        ///     Log as warnning level
        /// </summary>
        public void Warn(object message)
        {
            Console.WriteLine(message);
        }

        /// <summary>
        ///     Log as warnning level
        /// </summary>
        public void Warn(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        /// <summary>
        ///     Log exception
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="e"></param>
        public void Exception(string msg, Exception e)
        {
            Console.WriteLine($"{msg}:{e.InnerException?.Message ?? e.Message}:{e.StackTrace}");
        }

        /// <summary>
        ///     Destroy logger
        /// </summary>
        public void Shutdown()
        {
        }
    }
}
