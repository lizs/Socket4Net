using System;
using System.Diagnostics;

namespace socket4net
{
    public abstract class LogicServiceBase : Obj, ILogicService
    {
        protected const int StopWatchDivider = 128;
        protected bool StopWorking;
        protected readonly Stopwatch Watch = new Stopwatch();
        private int _previousCalcTime;

        /// <summary>
        ///     定时器调度器
        /// </summary>
        public TimerScheduler Scheduler { get; protected set; }
        
        /// <summary>
        /// 在本服务执行该Action
        /// </summary>
        /// <param name="action"></param>
        public void Perform(Action action)
        {
            Enqueue(action);
        }

        /// <summary>
        /// 在本服务执行该Action
        /// </summary>
        /// <param name="action"></param>
        /// <param name="param"></param>
        public void Perform<T>(Action<T> action, T param)
        {
            Enqueue(action, param);
        }

        /// <summary>
        ///     协程调度器
        /// </summary>
        public CoroutineScheduler CoroutineScheduler { get; protected set; }

        /// <summary>
        ///     Idle event, call by working thread
        ///     every period.
        ///     <remarks>
        ///         generally the working thread
        ///         call the event every period, but if it's too busy
        ///         because the working item consumes too much time,
        ///         the calling period may grater than the original period
        ///     </remarks>
        /// </summary>
        public abstract event Action Idle;

        /// <summary>
        ///     Specify the capacity of the working item queue.
        ///     <remarks>
        ///         when the items count in working queue reach the capacity
        ///         of the queue, all producer will block until there is one or more slot being free
        ///     </remarks>
        /// </summary>
        public int QueueCapacity { get; protected set; }

        /// <summary>
        ///     Specify the working period of the working thread in milliseconds.
        ///     That is, every period,the working thread loop back to the working
        ///     procedure's top. and, the Idle event is called.
        /// </summary>
        public int Period { get; set; }

        /// <summary>
        ///     A time counter that count the work items consume how much time
        ///     in one working thread's loop. It maybe grater than the working period,
        ///     which indicates the work items consume too much time.
        /// </summary>
        public long WiElapsed { get; protected set; }

        /// <summary>
        ///     A time counter that count the Idle event callbacks consume how much
        ///     time in one working thread's loop. This value should be less than period.
        /// </summary>
        public long IdleCallbackElapsed { get; protected set; }

        /// <summary>
        ///     Specify the work items count currently in working queue.
        /// </summary>
        public abstract int Jobs { get; }

        /// <summary>
        /// 性能指标
        /// 每秒执行的Job
        /// </summary>
        public int ExcutedJobsPerSec { get; protected set; }

        /// <summary>
        /// Job队列容量
        /// 由上层指定
        /// </summary>
        public int Capacity { get; set; }
        
        /// <summary>
        ///     Get the elapsed milliseconds since the instance been constructed
        /// </summary>
        public long ElapsedMilliseconds
        {
            get { return Watch.ElapsedMilliseconds; }
        }

        /// <summary>
        ///     External(e.g. the TCP socket thread) call this method to push
        ///     a work item into the working queue. The work item must not
        ///     be null.
        /// </summary>
        /// <param name="w">the work item object, must not be null</param>
        public abstract void Enqueue(IJob w);

        /// <summary>
        ///     External(e.g. the TCP socket thread) call this method to push
        ///     a work item into the working queue.
        /// </summary>
        /// <param name="proc">the working procedure</param>
        /// <param name="param">additional parameter that passed to working procedure</param>
        public void Enqueue<T>(Action<T> proc, T param)
        {
            var w = new Job<T>(proc, param);
            Enqueue(w);
        }

        /// <summary>
        /// 入队
        /// </summary>
        /// <param name="proc"></param>
        public void Enqueue(Action proc)
        {
            var job = new Job(proc);
            Enqueue(job);
        }

        /// <summary>
        /// 计算性能
        /// </summary>
        protected void CalcPerformance()
        {
            var delta = (Environment.TickCount - _previousCalcTime)/1000.0f;
            if (delta < 1.0f)
                ++ExcutedJobsPerSec;
            else
            {
                ExcutedJobsPerSec = (int) (ExcutedJobsPerSec/delta);
                _previousCalcTime = Environment.TickCount;
                ExcutedJobsPerSec = 0;
            }
        }

        protected override void OnStart()
        {
            base.OnStart();
            QueueCapacity = Capacity;
            Scheduler = new TimerScheduler(this);
            CoroutineScheduler = Create<CoroutineScheduler>(new ObjArg(this));
            CoroutineScheduler.Start();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Scheduler.Dispose();
            CoroutineScheduler.Destroy();
        }
    }
}