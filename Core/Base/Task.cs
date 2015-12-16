using System;
using System.Collections;

namespace socket4net
{
    public abstract class TaskBase : Obj, IEnumerable
    {
        public string Id { get { return Name; } }

        private bool _isDone;
        private bool _failed;

        public event Action<string> EventFailed;
        public event Action EventCompleted;

        public virtual bool IsDone
        {
            get { return _isDone; }
            set
            {
                if (value == _isDone) return;
                _isDone = value;
                if (_isDone && EventCompleted != null)
                    EventCompleted();
            }
        }

        public bool Failed
        {
            get { return _failed; }
            protected set
            {
                if (_failed == value) return;
                _failed = value;

                if (_failed && EventFailed != null)
                    EventFailed("");
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // 清理事件
            EventCompleted = null;
            EventFailed = null;
        }

        public abstract IEnumerator Step();
        public IEnumerator GetEnumerator()
        {
            return Step();
        }
    }

    /// <summary>
    /// 模糊异步任务
    /// 任务在不确定步骤内完成
    /// </summary>
    public abstract class FuzzyTask : TaskBase
    {
        
    }

    /// <summary>
    /// 精准异步任务
    /// 任务在固定步骤内完成
    /// </summary>
    public abstract class StrictTask : FuzzyTask
    {
        public event Action<int, int> EventProgressChanged;

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventProgressChanged = null;
        }

        int _progress;
        public virtual int Progress
        {
            get { return _progress; }
            protected set
            {
                if (value == _progress) return;

                _progress = value;
                if (EventProgressChanged != null)
                    EventProgressChanged(Steps, Progress);

                if (_progress >= Steps)
                    IsDone = true;
            }
        }

        public int Steps { get; protected set; }
    }
}
