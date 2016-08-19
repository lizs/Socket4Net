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
    /// A timer class base on Linux style timer scheduler.
    /// </summary>
    public sealed class Timer
    {
        private readonly TimerScheduler _scheduler;

        /// <summary>
        /// Construct a new timer object.
        /// </summary>
        /// <param name="name">the timer's name</param>
        /// <param name="dueTime">when to begin this timer, in milliseconds.
        /// zero means start immediately</param>
        /// <param name="period">the period of this timer, , in milliseconds.
        /// zero means the timer is schedule once only</param>
        public Timer(string name, uint dueTime, uint period)
            : this(name, dueTime)
        {
            Period = period;
        }

        /// <summary>
        /// Construct a new timer object.
        /// </summary>
        /// <param name="name">the timer's name</param>
        /// <param name="dueTime">when to begin this timer, in milliseconds.
        /// zero means start immediately</param>
        public Timer(string name, uint dueTime)
        {
            Name = name;
            DueTime = dueTime;
            _scheduler = GlobalVarPool.Ins.LogicService.Scheduler;
        }

        /// <summary>
        /// Time's name
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// When to start the timer, in milliseconds.
        /// </summary>
        public uint DueTime
        {
            get;
            set;
        }

        /// <summary>
        /// The timer period, in milliseconds.
        /// </summary>
        public uint Period
        {
            get;
            set;
        }

        /// <summary>
        /// Indicate it's a period timer or not.
        /// </summary>
        public bool IsPeriod => Period > 0;

        /// <summary>
        /// Call the timer's callback events.
        /// This method is called by <c>TimerScheduler</c> class only.
        /// </summary>
        internal void Trigger()
        {
            if (Arrived != null)
            {
                try
                {
                    Arrived(this);
                }
                catch (Exception e)
                {
                    Logger.Ins.Error("Timer exception, {0} : {1}", e.Message, e.StackTrace);
                }
            }
        }

        /// <summary>
        /// Indicate whether the timer is start or not.
        /// </summary>
        public bool IsStarted => Entry != null;

        /// <summary>
        /// User define parameter.
        /// </summary>
        public object State
        {
            get;
            set;
        }

        /// <summary>
        /// The expire time.
        /// this property only be used by <c>TimerScheduler</c> class.
        /// </summary>
        internal long Expires
        {
            get;
            set;
        }
        /// <summary>
        /// When timer is started, it's put in queue, this property is
        /// the queue entry for this timer object.
        /// this property only be used by <c>TimerScheduler</c> class.
        /// </summary>
        internal QueueEntry Entry
        {
            get;
            set;
        }
        /// <summary>
        /// When timer is trigger, this event will be called.
        /// </summary>
        public event Action<Timer> Arrived;

        /// <summary>
        /// Start the timer.
        /// </summary>
        public void Start()
        {
            _scheduler.Add(this);
        }

        /// <summary>
        /// Stop the timer.
        /// </summary>
        public void Stop()
        {
            State = null;
            _scheduler.Remove(this);
            Arrived = null;
        }
    }
}
