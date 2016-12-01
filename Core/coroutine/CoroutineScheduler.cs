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

        /// <summary>
        ///     internal called when an Obj is initialized
        /// </summary>
        /// <param name="arg"></param>
        protected override void OnInit(ObjArg arg)
        {
            base.OnInit(arg);
            GlobalVarPool.Ins.Service.Idle += OnIdle;
        }

        /// <summary>
        ///     internal called when an Obj is to be destroyed
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();
            GlobalVarPool.Ins.Service.Idle -= OnIdle;
        }

        private void OnIdle()
        {
            Update();
        }

        /// <summary>
        /// </summary>
        /// <param name="coroutine"></param>
        public void InternalStopCoroutine(Coroutine coroutine)
        {
            _dead.Remove(coroutine);
            _coroutines.Remove(coroutine);
        }

        /// <summary>
        /// </summary>
        /// <param name="fun"></param>
        /// <returns></returns>
        public Coroutine InternalStartCoroutine(Func<IEnumerator> fun)
        {
            var co = new Coroutine(fun);
            _coroutines.AddLast(co);
            return co;
        }

        /// <summary>
        /// </summary>
        /// <param name="fun"></param>
        /// <param name="arg"></param>
        /// <returns></returns>
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