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

namespace socket4net
{
    /// <summary>
    ///     自动计时器
    /// </summary>
    public class AutoWatch : IDisposable
    {
        private readonly Stopwatch _watch = new Stopwatch();
        public AutoWatch(string name, long threhold = 50, Action<string, long> handler = null)
        {
            Name = name;
            Handler = handler;
            Threhold = threhold;
            _watch.Start();
        }

        public string Name { get; private set; }
        public long Threhold { get; private set; }
        public Action<string, long> Handler { get; private set; }

        public void Dispose()
        {
            _watch.Stop();
            if (_watch.ElapsedMilliseconds > Threhold)
                Logger.Ins.Warn("{0} : {1} ms", Name, _watch.ElapsedMilliseconds);

            if (Handler != null)
                Handler(Name, _watch.ElapsedMilliseconds);
        }

        public long ElapsedMilliseconds { get { return _watch.ElapsedMilliseconds; } }
    }
}
