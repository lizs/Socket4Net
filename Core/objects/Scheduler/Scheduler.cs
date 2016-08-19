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
using System.Collections.Generic;
using System.Linq;

namespace socket4net
{
    /// <summary>
    ///     timer scheduler
    /// </summary>
    public class Scheduler : Obj
    {
        private readonly Dictionary<Action, TimerWrapper> _timers = new Dictionary<Action, TimerWrapper>();

        /// <summary>
        ///     add timer 
        ///     used internal
        /// </summary>
        /// <param name="action"></param>
        /// <param name="timer"></param>
        protected void AddTimer(Action action, TimerWrapper timer) => _timers.Add(action, timer);

        /// <summary>
        ///    internal called when an Obj is to be destroyed
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();
            Clear();
        }

        /// <summary>
        ///     clear the timers maintained by me
        /// </summary>
        public void Clear()
        {
            foreach (var timer in _timers.Select(x => x.Value))
                timer.Stop();
            _timers.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="delay"></param>
        /// <param name="period"></param>
        public virtual void InvokeRepeatingImp(Action action, uint delay, uint period)
        {
            CancelInvokeImp(action);

            var timer = TimerWrapper.New(Name, action, delay, period);
            _timers.Add(action, timer);
            timer.Start();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="delay"></param>
        public virtual void InvokeImp(Action action, uint delay)
        {
            CancelInvokeImp(action);

            var timer = TimerWrapper.New(Name, action, delay);
            _timers.Add(action, timer);
            timer.Start();
        }

        /// <summary>
        ///     
        /// </summary>
        /// <param name="action"></param>
        public void CancelInvokeImp(Action action)
        {
            if (!_timers.ContainsKey(action)) return;

            var timer = _timers[action];
            timer.Stop();
            _timers.Remove(action);
        }
    }
}
