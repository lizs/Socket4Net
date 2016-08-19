using System;

namespace socket4net
{
    /// <summary>
    ///     sliced object
    /// </summary>
    public abstract class SlicedObj : Obj
    {
        /// <summary>
        ///     event raised when progress changed
        /// </summary>
        public event Action<int, int> EventProgressChanged;

        /// <summary>
        ///     event raised when process failed
        /// </summary>
        public event Action<string> EventFailed;

        /// <summary>
        ///     event raised when process completed
        /// </summary>
        public event Action EventCompleted;

        private bool _isDone;
        private bool _failed;
        private int _progress;

        /// <summary>
        ///    get process percentage
        /// </summary>
        public float Percent => Progress / (float)Steps;

        /// <summary>
        ///     get process progress
        /// </summary>
        public int Progress
        {
            get { return _progress; }
            protected set
            {
                if (value == _progress) return;

                _progress = value;
                EventProgressChanged?.Invoke(Steps, Progress);
            }
        }

        /// <summary>
        ///     get sliced steps
        /// </summary>
        public int Steps { get; protected set; } = 1;

        /// <summary>
        ///     specify wether processing is done
        /// </summary>
        public bool IsDone
        {
            get { return _isDone; }
            protected set
            {
                if (value == _isDone) return;
                _isDone = value;
                if (_isDone)
                    EventCompleted?.Invoke();
            }
        }

        /// <summary>
        ///     specify wether processing is failed
        /// </summary>
        public bool Failed
        {
            get { return _failed; }
            protected set
            {
                if (_failed == value) return;
                _failed = value;

                if (_failed)
                    EventFailed?.Invoke("");
            }
        }

        /// <summary>
        ///    internal called when an Obj is to be destroyed
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();

            // 清理事件
            EventCompleted = null;
            EventFailed = null;
            EventProgressChanged = null;
        }
    }
}
