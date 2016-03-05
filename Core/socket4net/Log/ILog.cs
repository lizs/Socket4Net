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
