using System;
using System.Collections;

namespace socket4net
{
    /// <summary>
    ///     协程
    /// </summary>
    public sealed class Coroutine
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
}

