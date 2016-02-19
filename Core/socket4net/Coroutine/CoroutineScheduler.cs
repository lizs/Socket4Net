using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace socket4net
{
    /// <summary>
    ///     协程调度器
    /// </summary>
    public sealed class CoroutineScheduler : Obj
    {
        private readonly LinkedList<Coroutine> _coroutines = new LinkedList<Coroutine>();
        private readonly LinkedList<Coroutine> _dead = new LinkedList<Coroutine>();

        protected override void OnInit(ObjArg arg)
        {
            base.OnInit(arg);
            GlobalVarPool.Ins.LogicService.Idle += OnIdle;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            GlobalVarPool.Ins.LogicService.Idle -= OnIdle;
        }

        private void OnIdle()
        {
            Update();
        }

        public void InternalStopCoroutine(Coroutine coroutine)
        {
            _dead.Remove(coroutine);
            _coroutines.Remove(coroutine);
        }

        public Coroutine InternalStartCoroutine(Func<IEnumerator> fun)
        {
            var co = new Coroutine(fun);
            _coroutines.AddLast(co);
            return co;
        }

        public Coroutine InternalStartCoroutine(Func<object[], IEnumerator> fun, params object[] arg)
        {
            var co = new Coroutine(fun, arg);
            _coroutines.AddLast(co);
            return co;
        }

        private void Update()
        {
            foreach (var coroutine in _dead)
                _coroutines.Remove(coroutine);

            _dead.Clear();

            // 拷贝一份，以防止协程迭代器破坏
            foreach (var coroutine in _coroutines.ToList())
            {
                if (!coroutine.Update())
                    _dead.AddLast(coroutine);
            }
        }
    }
}