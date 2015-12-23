using System;
using System.Collections;
using System.Collections.Generic;

namespace socket4net
{
    /// <summary>
    ///     协程调度器
    /// </summary>
    public class CoroutineScheduler : Obj
    {
        /// <summary>
        ///     协程
        /// </summary>
        class Coroutine
        {
            private IEnumerator _enumerator;
            private bool _completed;

            private bool Completed
            {
                get { return _completed; }
                set
                {
                    _completed = value;
                    if (_completed)
                        _enumerator = null;
                }
            }

            public Coroutine(Func<object[], IEnumerator> fun, params object[] args)
            {
                if (fun == null) throw new ArgumentException("enumerator is null");
                _enumerator = fun(args);
            }

            public Coroutine(Func<IEnumerator> fun)
            {
                if (fun == null) throw new ArgumentException("enumerator is null");
                _enumerator = fun();
            }

            /// <summary>
            ///     每帧执行一次
            /// </summary>
            /// <returns></returns>
            public bool Update()
            {
                if (Completed) return false;
                if (Process(_enumerator))
                    return true;

                Completed = true;
                return false;
            }

            /// <summary>
            ///     处理枚举器
            /// </summary>
            /// <param name="enumerator"></param>
            /// <returns>
            ///     False表示当前无任务
            ///     True表示还有任务需处理
            /// </returns>
            private static bool Process(IEnumerator enumerator)
            {
                if (enumerator == null) return false;

                bool result;
                var subEnumerator = enumerator.Current as IEnumerator;
                if (subEnumerator != null)
                {
                    result = Process(subEnumerator);
                    if (!result)
                        result = enumerator.MoveNext();
                }
                else
                    result = enumerator.MoveNext();

                return result;
            }
        }
        
        private readonly LinkedList<Coroutine> _coroutines = new LinkedList<Coroutine>();
        private readonly LinkedList<Coroutine> _dead = new LinkedList<Coroutine>();

        protected override void OnInit(ObjArg arg)
        {
            base.OnInit(arg);
            var service = Owner as ILogicService;
            if(service == null)
                throw new Exception("Owner logic service is null");

            service.Idle += OnIdle;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            var service = Owner as ILogicService;
            if (service != null)
                service.Idle -= OnIdle;
        }

        private void OnIdle()
        {
            Update();
        }

        public void StartCoroutine(Func<IEnumerator> fun)
        {
            var co = new Coroutine(fun);
            _coroutines.AddLast(co);
        }

        public void StartCoroutine(Func<object[], IEnumerator> fun, params object[] arg)
        {
            var co = new Coroutine(fun, arg);
            _coroutines.AddLast(co);
        }

        private void Update()
        {
            foreach (var coroutine in _dead)
                _coroutines.Remove(coroutine);

            _dead.Clear();

            foreach (var coroutine in _coroutines)
            {
                if (!coroutine.Update())
                    _dead.AddLast(coroutine);
            }
        }
    }
}
