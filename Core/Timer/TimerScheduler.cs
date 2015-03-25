/********************************************************************
 *  created:    2011/12/21   3:57
 *  filename:   TimerScheduler.cs
 *
 *  author:     Linguohua
 *  copyright(c) 2011
 *
 *  purpose:    Implements a Linux style timer scheduler.
 *      The code are partial migrate from Linux core.
 *      See Linux/Kernel/Timer.c file for details.
*********************************************************************/

using System;
using System.Diagnostics;
using socket4net.Service;

namespace socket4net.Timer
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

        public bool IsHead
        {
            get { return Timer == null; }
        }
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
    static class TimerConstant
    {
        public const int TVN_BITS = 6;
        public const int TVN_SIZE = (1 << TVN_BITS);
        public const int TVR_BITS = 8;
        public const int TVR_SIZE = (1 << TVR_BITS);
        public const int TVN_MASK = (TVN_SIZE - 1);
        public const int TVR_MASK = (TVR_SIZE - 1);
    }

    /// <summary>
    /// Timer vector class.
    /// </summary>
    class TVN
    {
        public TVN()
        {
            for (int i = 0; i < TimerConstant.TVN_SIZE; ++i)
            {
                _Vector[i] = new QueueEntry();
            }
        }

        private QueueEntry[] _Vector
            = new QueueEntry[TimerConstant.TVN_SIZE];
        public QueueEntry this[int index]
        {
            get { return _Vector[index]; }
        }

        public QueueEntry[] TV
        {
            get { return _Vector; }
        }
    }

    /// <summary>
    /// Timer vector class.
    /// It's a specific type of <c>TVN</c>, it's vector size
    /// is different from <c>TVN</c>.
    /// </summary>
    class TVR
    {
        public TVR()
        {
            for (int i = 0; i < TimerConstant.TVR_SIZE; ++i)
            {
                _Vector[i] = new QueueEntry();
            }
        }
        private QueueEntry[] _Vector
            = new QueueEntry[TimerConstant.TVR_SIZE];

        public QueueEntry this[int index]
        {
            get { return _Vector[index]; }
        }

        public QueueEntry[] TV
        {
            get { return _Vector; }
        }
    }

    /// <summary>
    /// A Linux style timer scheduler
    /// </summary>
    public class TimerScheduler : IDisposable
    {
        public LogicService HostService { get; private set; }

        public TimerScheduler(LogicService service)
        {
            HostService = service;

            HostService.Idle += RunTimer;
            _TimerJiffies = service.ElapsedMilliseconds;
        }

        public void Dispose()
        {
            HostService.Idle -= RunTimer;
        }
        
        /// <summary>
        /// This field used to store how many milliseconds
        /// elapsed since this scheduler been constructed.
        /// All timer's expire time is calculated base on this field.
        /// </summary>
        private long _TimerJiffies;

        /// <summary>
        /// When scheduler is trigger a timer, this field store the timer object.
        /// </summary>
        private Timer _RunningTimer;
        private bool _DeletedRunningTimer;

        /// <summary>
        /// The first class of timers vector, their expire time will less than 256 milliseconds.
        /// </summary>
        private TVR _TV1 = new TVR();
        /// <summary>
        /// The second class of timers vector, their expire time will grater than 255 milliseconds
        /// and less than 16384(1 left shit 14) milliseconds
        /// </summary>
        private TVN _TV2 = new TVN();
        /// <summary>
        /// The second class of timers vector, their expire time will grater than 16384 milliseconds
        /// and less than 1048576(1 left shit 20) milliseconds
        /// </summary>
        private TVN _TV3 = new TVN();
        /// <summary>
        /// The second class of timers vector, their expire time will grater than 1048576 milliseconds
        /// and less than 67108864(1 left shit 26) milliseconds
        /// </summary>
        private TVN _TV4 = new TVN();
        /// <summary>
        /// The second class of timers vector, their expire time will grater than 67108864 milliseconds
        /// and less than uint.Max(1 left shit 32) milliseconds
        /// if the expires is grater then uint.Max, it will be truncated.
        /// </summary>
        private TVN _TV5 = new TVN();
        
        /// <summary>
        /// <c>StaService</c>'s Idle event call this method
        /// to check all expired timers.
        /// </summary>
        private void RunTimer()
        {
            var jiffes = HostService.ElapsedMilliseconds; 
            while (jiffes - _TimerJiffies >= 0)
            {
                int index = (int)(_TimerJiffies & TimerConstant.TVR_MASK);
                if (index == 0)
                {
                    Cascade();
                }

                ++_TimerJiffies;

                var h = _TV1[index];
                while (h.Next != h)
                {
                    var e = h.Next;
                    e.DeQueue();

                    _DeletedRunningTimer = false;

                    _RunningTimer = e.Timer;
                    _RunningTimer.Trigger();

                    if (!_DeletedRunningTimer && _RunningTimer.Period > 0)
                    {
                        _RunningTimer.Expires = _TimerJiffies + _RunningTimer.Period;
                        InternalAdd(e);
                    }
                    else
                    {
                        e.Discard();
                    }
                }
            }

            _RunningTimer = null;
            _DeletedRunningTimer = false;
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

            t.Expires = t.DueTime + _TimerJiffies;
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

            if (t == _RunningTimer)
            {
                _DeletedRunningTimer = true;
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
        private int Cascade(TVN tv, int index)
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
            if (Cascade(_TV2, (int)((_TimerJiffies >> (TimerConstant.TVR_BITS)) & TimerConstant.TVN_MASK)) == 0)
            {
                if (Cascade(_TV3, (int)((_TimerJiffies >> (TimerConstant.TVR_BITS + TimerConstant.TVN_BITS)) & TimerConstant.TVN_MASK)) == 0)
                {
                    if (Cascade(_TV4, (int)((_TimerJiffies >> (TimerConstant.TVR_BITS + 2 * TimerConstant.TVN_BITS)) & TimerConstant.TVN_MASK)) == 0)
                    {
                        Cascade(_TV5, (int)((_TimerJiffies >> (TimerConstant.TVR_BITS + 3 * TimerConstant.TVN_BITS)) & TimerConstant.TVN_MASK));
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
            long expires = e.Timer.Expires;
            long idx = expires - _TimerJiffies;

            int ti;
            QueueEntry[] tv;
            if (idx < 0)
            {
                ti = (int)(_TimerJiffies & TimerConstant.TVR_MASK);
                tv = _TV1.TV;
            }
            else if (idx < TimerConstant.TVR_SIZE)
            {
                ti = (int)(expires & TimerConstant.TVR_MASK);
                tv = _TV1.TV;
            }
            else if (idx < (1 << (TimerConstant.TVR_BITS + TimerConstant.TVN_BITS)))
            {
                ti = (int)((expires >> TimerConstant.TVR_BITS) & TimerConstant.TVN_MASK);
                tv = _TV2.TV;
            }
            else if (idx < (1 << (TimerConstant.TVR_BITS + 2 * TimerConstant.TVN_BITS)))
            {
                ti = (int)((expires >> (TimerConstant.TVR_BITS + TimerConstant.TVN_BITS)) & TimerConstant.TVN_MASK);
                tv = _TV3.TV;
            }
            else if (idx < (1 << (TimerConstant.TVR_BITS + 3 * TimerConstant.TVN_BITS)))
            {
                ti = (int)((expires >> (TimerConstant.TVR_BITS + 2 * TimerConstant.TVN_BITS)) & TimerConstant.TVN_MASK);
                tv = _TV4.TV;
            }
            else
            {
                ti = (int)((expires >> (TimerConstant.TVR_BITS + 3 * TimerConstant.TVN_BITS)) & TimerConstant.TVN_MASK);
                tv = _TV5.TV;
            }

            var h = tv[ti];
            e.QueueTail(h);
        }
    }
}
