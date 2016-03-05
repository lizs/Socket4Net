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
    public class Scheduler : Obj
    {
        protected readonly Dictionary<Action, TimerWrapper> Timers = new Dictionary<Action, TimerWrapper>();

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Clear();
        }

        public void Clear()
        {
            foreach (var timer in Timers.Select(x => x.Value))
                timer.Stop();
            Timers.Clear();
        }

        public virtual void InvokeRepeating(Action action, uint delay, uint period)
        {
            CancelInvoke(action);

            var timer = TimerWrapper.New(Name, action, delay, period);
            Timers.Add(action, timer);
            timer.Start();
        }

        public virtual void Invoke(Action action, uint delay)
        {
            CancelInvoke(action);

            var timer = TimerWrapper.New(Name, action, delay);
            Timers.Add(action, timer);
            timer.Start();
        }

        public void CancelInvoke(Action action)
        {
            if (!Timers.ContainsKey(action)) return;

            var timer = Timers[action];
            timer.Stop();
            Timers.Remove(action);
        }
    }
}
