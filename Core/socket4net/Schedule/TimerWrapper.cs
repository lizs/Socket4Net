/********************************************************************
 *  created:    2011/12/21   3:59
 *  filename:   TimerWrapper.cs
 *
 *  author:     Linguohua
 *  copyright(c) 2011
 *
 *  purpose:    TimerWrapper class. A timer class base on Linux style
 *      timer scheduler.
*********************************************************************/

using System;

namespace socket4net
{
    /// <summary>
    /// A timer class base on Linux style timer scheduler.
    /// </summary>
    public sealed class TimerWrapper : IDisposable
    {
        private readonly TimerScheduler _scheduler;

        /// <summary>
        /// Construct a new timer object.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="name">the timer's name</param>
        /// <param name="dueTime">when to begin this timer, in milliseconds.
        /// zero means start immediately</param>
        /// <param name="period">the period of this timer, , in milliseconds.
        /// zero means the timer is schedule once only</param>
        public TimerWrapper(string name, uint dueTime, uint period)
            : this(name, dueTime)
        {
            Period = period;
        }

        /// <summary>
        /// Construct a new timer object.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="name">the timer's name</param>
        /// <param name="dueTime">when to begin this timer, in milliseconds.
        /// zero means start immediately</param>
        public TimerWrapper(string name, uint dueTime)
        {
            Name = name;
            DueTime = dueTime;
            _scheduler = GlobalVarPool.Instance.LogicService.Scheduler;
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
        public bool IsPeriod
        {
            get { return Period > 0; }
        }

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
                    Logger.Instance.ErrorFormat("TimerWrapper exception, {0} : {1}", e.Message, e.StackTrace);
                }
            }
        }

        /// <summary>
        /// Indicate whether the timer is start or not.
        /// </summary>
        public bool IsStarted
        {
            get { return Entry != null; }
        }

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
        public event Action<TimerWrapper> Arrived;

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

        /// <summary>
        /// Dispose this timer
        /// </summary>
        public void Dispose()
        {
            Stop();
        }
    }
}
