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

namespace socket4net
{
    /// <summary>
    ///     协程
    /// </summary>
    public sealed class Coroutine
    {
        private bool _completed;
        private IEnumerator _enumerator;

        /// <summary>
        ///     constructor
        /// </summary>
        /// <param name="fun"></param>
        /// <param name="args"></param>
        /// <exception cref="ArgumentException"></exception>
        public Coroutine(Func<object[], IEnumerator> fun, params object[] args)
        {
            if (fun == null) throw new ArgumentException("enumerator is null");
            _enumerator = fun(args);
        }

        /// <summary>
        ///     constructor
        /// </summary>
        /// <param name="fun"></param>
        /// <exception cref="ArgumentException"></exception>
        public Coroutine(Func<IEnumerator> fun)
        {
            if (fun == null) throw new ArgumentException("enumerator is null");
            _enumerator = fun();
        }

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

        /// <summary>
        ///     每帧执行一次
        /// </summary>
        /// <returns>false表示协程未完毕，反之协程已完毕</returns>
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