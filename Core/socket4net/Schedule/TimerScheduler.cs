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
    /// Queue entry class
    /// </summary>
    class QueueEntry
    {
        /// <summary>
        /// Construct a new entry object with a timer object.
        /// </summary>
        /// <param name="t">a non-null timer object</param>
        public QueueEntry(Timer t)
        {
            // bind timer object t to this entry
            Timer = t;
            t.Entry = this;
        }

        /// <summary>
        /// Construct a new head entry. That is, it's Prev/Next properties
        /// are point to itself.
        /// </summary>
        public QueueEntry()
        {
            Next = this;
            Prev = this;
        }
        /// <summary>
        /// The Preview entry object
        /// </summary>
        public QueueEntry Prev
        {
            get;
            private set;
        }
        /// <summary>
        /// The next entry object
        /// </summary>
        public QueueEntry Next
        {
            get;
            private set;
        }
        /// <summary>
        /// The timer object
        /// </summary>
        public Timer Timer
        {
            get;
            private set;
        }

        public bool IsHead => Timer == null;

        /// <summary>
        /// Queue the entry object into a queue specify by 'h'
        /// </summary>
        /// <param name="h">the entry object, must be a head entry object</param>
        public void QueueTail(QueueEntry h)
        {
            Debug.Assert(Next == null && Prev == null && h.IsHead);
            Next = h;
            Prev = h.Prev;
            h.Prev.Next = this;
            h.Prev = this;
        }
        /// <summary>
        /// Dequeue the entry object
        /// </summary>
        public void DeQueue()
        {
            Prev.Next = Next;
            Next.Prev = Prev;
            Prev = null;
            Next = null;
        }
        /// <summary>
        /// UnBinding the timer object
        /// </summary>
        public void Discard()
        {
            Timer.Entry = null;
            Timer = null;
        }
    }

    /// <summary>
    /// Define the constant variable used by <c>TimerScheduler</c> class
    /// </summary>
    internal static class TimerConstant
    {
        public const int TvnBits = 6;
        public const int TvnSize = (1 << TvnBits);
        public const int TvrBits = 8;
        public const int TvrSize = (1 << TvrBits);
        public const int TvnMask = (TvnSize - 1);
        public const int TvrMask = (TvrSize - 1);
    }

    /// <summary>
    /// Timer vector class.
    /// </summary>
    internal class Tvn
    {
        public Tvn()
        {
            for (var i = 0; i < TimerConstant.TvnSize; ++i)
            {
                _vector[i] = new QueueEntry();
            }
        }

        private readonly QueueEntry[] _vector
            = new QueueEntry[TimerConstant.TvnSize];
        public QueueEntry this[int index] => _vector[index];

        public QueueEntry[] TV => _vector;
    }

    /// <summary>
    /// Timer vector class.
    /// It's a specific type of <c>TVN</c>, it's vector size
    /// is different from <c>TVN</c>.
    /// </summary>
    internal class Tvr
    {
        public Tvr()
        {
            for (var i = 0; i < TimerConstant.TvrSize; ++i)
            {
                _vector[i] = new QueueEntry();
            }
        }
        private readonly QueueEntry[] _vector
            = new QueueEntry[TimerConstant.TvrSize];

        public QueueEntry this[int index] => _vector[index];

        public QueueEntry[] TV => _vector;
    }

    /// <summary>
    /// A Linux style timer scheduler
    /// </summary>
    public class TimerScheduler : IDisposable
    {
        /// <summary>
        /// get the logic service where this scheduler hosted
        /// </summary>
        public ILogicService HostService { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="service"></param>
        public TimerScheduler(ILogicService service)
        {
            HostService = service;

            HostService.Idle += RunTimer;
            _timerJiffies = service.ElapsedMilliseconds;
        }

        /// <summary>执行与释放或重置非托管资源相关的应用程序定义的任务。</summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            HostService.Idle -= RunTimer;
        }
        
        /// <summary>
        /// This field used to store how many milliseconds
        /// elapsed since this scheduler been constructed.
        /// All timer's expire time is calculated base on this field.
        /// </summary>
        private long _timerJiffies;

        /// <summary>
        /// When scheduler is trigger a timer, this field store the timer object.
        /// </summary>
        private Timer _runningTimer;
        private bool _deletedRunningTimer;

        /// <summary>
        /// The first class of timers vector, their expire time will less than 256 milliseconds.
        /// </summary>
        private readonly Tvr _tv1 = new Tvr();
        /// <summary>
        /// The second class of timers vector, their expire time will grater than 255 milliseconds
        /// and less than 16384(1 left shit 14) milliseconds
        /// </summary>
        private readonly Tvn _tv2 = new Tvn();
        /// <summary>
        /// The second class of timers vector, their expire time will grater than 16384 milliseconds
        /// and less than 1048576(1 left shit 20) milliseconds
        /// </summary>
        private readonly Tvn _tv3 = new Tvn();
        /// <summary>
        /// The second class of timers vector, their expire time will grater than 1048576 milliseconds
        /// and less than 67108864(1 left shit 26) milliseconds
        /// </summary>
        private readonly Tvn _tv4 = new Tvn();
        /// <summary>
        /// The second class of timers vector, their expire time will grater than 67108864 milliseconds
        /// and less than uint.Max(1 left shit 32) milliseconds
        /// if the expires is grater then uint.Max, it will be truncated.
        /// </summary>
        private readonly Tvn _tv5 = new Tvn();
        
        /// <summary>
        /// <c>StaService</c>'s Idle event call this method
        /// to check all expired timers.
        /// </summary>
        private void RunTimer()
        {
            var jiffes = HostService.ElapsedMilliseconds; 
            while (jiffes - _timerJiffies >= 0)
            {
                var index = (int)(_timerJiffies & TimerConstant.TvrMask);
                if (index == 0)
                {
                    Cascade();
                }

                ++_timerJiffies;

                var h = _tv1[index];
                while (h.Next != h)
                {
                    var e = h.Next;
                    e.DeQueue();

                    _deletedRunningTimer = false;

                    _runningTimer = e.Timer;
                    _runningTimer.Trigger();

                    if (!_deletedRunningTimer && _runningTimer.Period > 0)
                    {
                        _runningTimer.Expires = _timerJiffies + _runningTimer.Period;
                        InternalAdd(e);
                    }
                    else
                    {
                        e.Discard();
                    }
                }
            }

            _runningTimer = null;
            _deletedRunningTimer = false;
        }
        /// <summary>
        /// Put a timer object in scheduler's queue.
        /// </summary>
        /// <param name="t">the timer object</param>
        public void Add(Timer t)
        {
            if (t.IsStarted)
            {
                return;
            }

            t.Expires = t.DueTime + _timerJiffies;
            InternalAdd(new QueueEntry(t));
        }
        /// <summary>
        /// remove a timer object from scheduler's queue.
        /// </summary>
        /// <param name="t">the timer object</param>
        public void Remove(Timer t)
        {
            if (!t.IsStarted)
            {
                return;
            }

            if (t == _runningTimer)
            {
                _deletedRunningTimer = true;
                return;
            }

            var e = t.Entry;
            e.DeQueue();
            e.Discard();
        }

        /// <summary>
        /// Cascade a timers vector
        /// </summary>
        /// <param name="tv">the timers vector</param>
        /// <param name="index">which queue to cascade</param>
        /// <returns></returns>
        private int Cascade(Tvn tv, int index)
        {
            var t = tv[index];

            while (t.Next != t)
            {
                var t1 = t.Next;
                t1.DeQueue();

                InternalAdd(t1);
            }
            return index;
        }
        /// <summary>
        /// Cascade all timers vectors
        /// </summary>
        private void Cascade()
        {
            if (Cascade(_tv2, (int)((_timerJiffies >> (TimerConstant.TvrBits)) & TimerConstant.TvnMask)) == 0)
            {
                if (Cascade(_tv3, (int)((_timerJiffies >> (TimerConstant.TvrBits + TimerConstant.TvnBits)) & TimerConstant.TvnMask)) == 0)
                {
                    if (Cascade(_tv4, (int)((_timerJiffies >> (TimerConstant.TvrBits + 2 * TimerConstant.TvnBits)) & TimerConstant.TvnMask)) == 0)
                    {
                        Cascade(_tv5, (int)((_timerJiffies >> (TimerConstant.TvrBits + 3 * TimerConstant.TvnBits)) & TimerConstant.TvnMask));
                    }
                }
            }
        }
        /// <summary>
        /// Queue an entry object into a suitable timer queue.
        /// </summary>
        /// <param name="e">the entry object</param>
        private void InternalAdd(QueueEntry e)
        {
            var expires = e.Timer.Expires;
            var idx = expires - _timerJiffies;

            int ti;
            QueueEntry[] tv;
            if (idx < 0)
            {
                ti = (int)(_timerJiffies & TimerConstant.TvrMask);
                tv = _tv1.TV;
            }
            else if (idx < TimerConstant.TvrSize)
            {
                ti = (int)(expires & TimerConstant.TvrMask);
                tv = _tv1.TV;
            }
            else if (idx < (1 << (TimerConstant.TvrBits + TimerConstant.TvnBits)))
            {
                ti = (int)((expires >> TimerConstant.TvrBits) & TimerConstant.TvnMask);
                tv = _tv2.TV;
            }
            else if (idx < (1 << (TimerConstant.TvrBits + 2 * TimerConstant.TvnBits)))
            {
                ti = (int)((expires >> (TimerConstant.TvrBits + TimerConstant.TvnBits)) & TimerConstant.TvnMask);
                tv = _tv3.TV;
            }
            else if (idx < (1 << (TimerConstant.TvrBits + 3 * TimerConstant.TvnBits)))
            {
                ti = (int)((expires >> (TimerConstant.TvrBits + 2 * TimerConstant.TvnBits)) & TimerConstant.TvnMask);
                tv = _tv4.TV;
            }
            else
            {
                ti = (int)((expires >> (TimerConstant.TvrBits + 3 * TimerConstant.TvnBits)) & TimerConstant.TvnMask);
                tv = _tv5.TV;
            }

            var h = tv[ti];
            e.QueueTail(h);
        }
    }
}
