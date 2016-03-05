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
    /// 基于socket4net逻辑服务的定时器
    /// </summary>
    public class TimerWrapper
    {
        public static TimerWrapper New(string name, Action action, uint duetime)
        {
            return new TimerWrapper(name, action, duetime);
        }

        public static TimerWrapper New(string name, Action action, uint duetime, uint period)
        {
            return new TimerWrapper(name, action, duetime, period);
        }

        public void Start()
        {
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Arrived -= OnTimer;
            _timer.Stop();
        }

        private readonly Timer _timer;
        private readonly Action _action;

        private void OnTimer(Timer timer)
        {
            _action();
        }

        private TimerWrapper(string name, Action action, uint duetime)
        {
            _timer = new Timer(name, duetime);
            _timer.Arrived += OnTimer;
            _action = action;
        }

        private TimerWrapper(string name, Action action, uint duetime, uint period)
        {
            _timer = new Timer(name, duetime, period);
            _timer.Arrived += OnTimer;
            _action = action;
        }
    }
}
