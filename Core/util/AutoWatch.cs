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
    ///     Automatic time watcher
    /// </summary>
    public class AutoWatch : IDisposable
    {
        private readonly Stopwatch _watch = new Stopwatch();

        /// <summary>
        ///     constructor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="threhold"></param>
        /// <param name="handler"></param>
        public AutoWatch(string name, long threhold = 50, Action<string, long> handler = null)
        {
            Name = name;
            Handler = handler;
            Threhold = threhold;
            _watch.Start();
        }

        /// <summary>
        ///     Watcher name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        ///     Wather threhold 
        ///     Warnning will be raised when elapsed ms > threhold
        /// </summary>
        public long Threhold { get; private set; }

        /// <summary>
        ///     Callback when this watch disposed
        /// </summary>
        public Action<string, long> Handler { get; private set; }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            _watch.Stop();
            if (_watch.ElapsedMilliseconds > Threhold)
                Logger.Ins.Warn("{0} : {1} ms", Name, _watch.ElapsedMilliseconds);

            Handler?.Invoke(Name, _watch.ElapsedMilliseconds);
        }

        /// <summary>
        ///     Get elapsed ms since this watch constructed
        /// </summary>
        public long ElapsedMilliseconds => _watch.ElapsedMilliseconds;
    }
}
