using System;

namespace socket4net.Service
{
    public interface IJob
    {
        void Do();
    }

    public struct Job : IJob
    {
        private readonly Action _procedure;

        public Job(Action proc)
        {
            _procedure = proc;
        }

        public Action Procedure
        {
            get { return _procedure; }
        }

        public void Do()
        {
            Procedure();
        }
    }

    public struct Job<T> : IJob
    {
        private readonly Action<T> _procedure;
        private readonly T _param;

        public Job(Action<T> proc, T param)
        {
            _procedure = proc;
            _param = param;
        }
        
        public Action<T> Procedure
        {
            get { return _procedure; }
        }

        public T Param
        {
            get { return _param; }
        }

        public void Do()
        {
            Procedure(Param);
        }
    }
}
