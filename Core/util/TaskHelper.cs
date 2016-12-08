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
#if NET45
using System;
using System.Threading;
using System.Threading.Tasks;

namespace socket4net
{
    /// <summary>
    ///     .net Task扩展方法
    /// </summary>
    public static class TaskHelper
    {
        #region 将以回调形式返回的方法包装为一个task
        /// <summary>
        ///      将以回调形式返回的方法包装为一个task
        /// </summary>
        /// <param name="fun"></param>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public static async Task<TResult> WrapCallback<TResult>(Action<Action<TResult>> fun)
        {
            var tcs = new TaskCompletionSource<TResult>();
            try
            {
                fun(tcs.SetResult);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }

            return await tcs.Task;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="fun"></param>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public static async Task<TResult> WrapCallback<T, TResult>(T arg1, Action<T, Action<TResult>> fun)
        {
            var tcs = new TaskCompletionSource<TResult>();
            try
            {
                fun(arg1, tcs.SetResult);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }

            return await tcs.Task;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="fun"></param>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public static async Task<TResult> WrapCallback<T1, T2, TResult>(T1 arg1, T2 arg2, Action<T1, T2, Action<TResult>> fun)
        {
            var tcs = new TaskCompletionSource<TResult>();
            try
            {
                fun(arg1, arg2, tcs.SetResult);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }

            return await tcs.Task;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        /// <param name="fun"></param>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public static async Task<TResult> WrapCallback<T1, T2, T3, TResult>(T1 arg1, T2 arg2, T3 arg3, Action<T1, T2, T3, Action<TResult>> fun)
        {
            var tcs = new TaskCompletionSource<TResult>();
            try
            {
                fun(arg1, arg2, arg3, tcs.SetResult);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }

            return await tcs.Task;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        /// <param name="arg4"></param>
        /// <param name="fun"></param>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public static async Task<TResult> WrapCallback<T1, T2, T3, T4, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, Action<T1, T2, T3, T4, Action<TResult>> fun)
        {
            var tcs = new TaskCompletionSource<TResult>();
            try
            {
                fun(arg1, arg2, arg3, arg4, tcs.SetResult);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }

            return await tcs.Task;
        }
        #endregion

        #region 异步执行
        /// <summary>
        ///     异步执行
        /// </summary>
        public static async Task<TResult> ExcuteAsync<TResult>(Func<TResult> fun)
        {
            var tcs = new TaskCompletionSource<TResult>();
            ThreadPool.QueueUserWorkItem(state =>
            {
                try
                {
                    var ret = fun();
                    GlobalVarPool.Ins.Service.Perform(() => tcs.SetResult(ret));
                }
                catch (Exception ex)
                {
                    GlobalVarPool.Ins.Service.Perform(() => tcs.SetException(ex));
                }
            });

            return await tcs.Task;
        }

        /// <summary>
        ///     异步执行
        /// </summary>
        public static async Task<TResult> ExcuteAsync<T1, TResult>(T1 arg1, Func<T1, TResult> fun)
        {
            var tcs = new TaskCompletionSource<TResult>();
            ThreadPool.QueueUserWorkItem(state =>
            {
                try
                {
                    var ret = fun(arg1);
                    GlobalVarPool.Ins.Service.Perform(() => tcs.SetResult(ret));
                }
                catch (Exception ex)
                {
                    GlobalVarPool.Ins.Service.Perform(() => tcs.SetException(ex));
                }
            });

            return await tcs.Task;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="fun"></param>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public static async Task<TResult> ExcuteAsync<T1, T2, TResult>(T1 arg1, T2 arg2, Func<T1, T2, TResult> fun)
        {
            var tcs = new TaskCompletionSource<TResult>();

            ThreadPool.QueueUserWorkItem(state =>
            {
                try
                {
                    var ret = fun(arg1, arg2);
                    GlobalVarPool.Ins.Service.Perform(() => tcs.SetResult(ret));
                }
                catch (Exception ex)
                {
                    GlobalVarPool.Ins.Service.Perform(() => tcs.SetException(ex));
                }
            });

            return await tcs.Task;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        /// <param name="fun"></param>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public static async Task<TResult> ExcuteAsync<T1, T2, T3, TResult>(T1 arg1, T2 arg2, T3 arg3, Func<T1, T2, T3, TResult> fun)
        {
            var tcs = new TaskCompletionSource<TResult>();

            ThreadPool.QueueUserWorkItem(state =>
            {
                try
                {
                    var ret = fun(arg1, arg2, arg3);
                    GlobalVarPool.Ins.Service.Perform(() => tcs.SetResult(ret));
                }
                catch (Exception ex)
                {
                    GlobalVarPool.Ins.Service.Perform(() => tcs.SetException(ex));
                }
            });

            return await tcs.Task;
        }

        #endregion

        #region 异步执行并以回调形式返回结果

        /// <summary>
        ///     同步执行一个Task
        /// </summary>
        /// <param name="fun"></param>
        /// <typeparam name="TResult"></typeparam>
        public static void ExcuteTask<TResult>(Func<Task<TResult>> fun)
        {
            ThreadPool.QueueUserWorkItem(async state =>
            {
                try
                {
                    await fun();
                }
                catch (Exception ex)
                {
                    Logger.Ins.Error("ExcuteTaskAndReturnByCallback 失败，{0}:{1}", ex.Message, ex.StackTrace);
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fun"></param>
        /// <param name="cb"></param>
        /// <typeparam name="TResult"></typeparam>
        public static void ExcuteTaskAndReturnByCallback<TResult>(Func<Task<TResult>> fun, Action<TResult> cb)
        {
            ThreadPool.QueueUserWorkItem(async state =>
            {
                try
                {
                    var ret = await fun();
                    GlobalVarPool.Ins.Service.Perform(() => cb(ret));
                }
                catch (Exception ex)
                {
                    Logger.Ins.Error("ExcuteTaskAndReturnByCallback 失败，{0}:{1}", ex.Message, ex.StackTrace);
                    GlobalVarPool.Ins.Service.Perform(() => cb(default(TResult)));
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="fun"></param>
        /// <param name="cb"></param>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        public static void ExcuteTaskAndReturnByCallback<T1, TResult>(T1 arg1,
            Func<T1, Task<TResult>> fun, Action<TResult> cb)
        {
            ThreadPool.QueueUserWorkItem(async state =>
            {
                try
                {
                    var ret = await fun(arg1);
                    GlobalVarPool.Ins.Service.Perform(() => cb(ret));
                }
                catch (Exception ex)
                {
                    Logger.Ins.Error("ExcuteTaskAndReturnByCallback 失败，{0}:{1}", ex.Message, ex.StackTrace);
                    GlobalVarPool.Ins.Service.Perform(() => cb(default(TResult)));
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="fun"></param>
        /// <param name="cb"></param>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        public static void ExcuteTaskAndReturnByCallback<T1, T2, TResult>(T1 arg1, T2 arg2,
            Func<T1, T2, Task<TResult>> fun, Action<TResult> cb)
        {
            ThreadPool.QueueUserWorkItem(async state =>
            {
                try
                {
                    var ret = await fun(arg1, arg2);
                    GlobalVarPool.Ins.Service.Perform(() => cb(ret));
                }
                catch (Exception ex)
                {
                    Logger.Ins.Error("ExcuteTaskAndReturnByCallback 失败，{0}:{1}", ex.Message, ex.StackTrace);
                    GlobalVarPool.Ins.Service.Perform(() => cb(default(TResult)));
                }
            });
        }
        #endregion
    }
}

#endif