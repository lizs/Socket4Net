#if NET45
using System;
using System.Threading;
using System.Threading.Tasks;

namespace socket4net
{
    public static class TaskHelper
    {
        #region 将以回调形式返回的方法包装为一个task
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

        /// <summary>
        ///     等待目标对象的TWatchEvent事件发生，将callback以Task包装返回
        /// </summary>
        /// <typeparam name="TWatchEvent"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="target"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static Task<bool> Watch<TWatchEvent>(IWatchable<TWatchEvent> target, Func<TWatchEvent, bool> filter)
        {
            var tcs = new TaskCompletionSource<bool>();
            Watch<TWatchEvent> watch = null;
            try
            {
                Func<TWatchEvent, bool> cbWrapper = @event =>
                {
                    if (filter(@event))
                    {
                        tcs.SetResult(true);
                        watch.Destroy();

                        return true;
                    }

                    return false;
                };

                watch = new Watch<TWatchEvent>(target, cbWrapper);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
                watch.Destroy();
            }

            return tcs.Task;
        }

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
                    GlobalVarPool.Ins.LogicService.Perform(() => tcs.SetResult(ret));
                }
                catch (Exception ex)
                {
                    GlobalVarPool.Ins.LogicService.Perform(() => tcs.SetException(ex));
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
                    GlobalVarPool.Ins.LogicService.Perform(() => tcs.SetResult(ret));
                }
                catch (Exception ex)
                {
                    GlobalVarPool.Ins.LogicService.Perform(() => tcs.SetException(ex));
                }
            });

            return await tcs.Task;
        }

        public static async Task<TResult> ExcuteAsync<T1, T2, TResult>(T1 arg1, T2 arg2, Func<T1, T2, TResult> fun)
        {
            var tcs = new TaskCompletionSource<TResult>();

            ThreadPool.QueueUserWorkItem(state =>
            {
                try
                {
                    var ret = fun(arg1, arg2);
                    GlobalVarPool.Ins.LogicService.Perform(() => tcs.SetResult(ret));
                }
                catch (Exception ex)
                {
                    GlobalVarPool.Ins.LogicService.Perform(() => tcs.SetException(ex));
                }
            });

            return await tcs.Task;
        }

        public static async Task<TResult> ExcuteAsync<T1, T2, T3, TResult>(T1 arg1, T2 arg2, T3 arg3, Func<T1, T2, T3, TResult> fun)
        {
            var tcs = new TaskCompletionSource<TResult>();

            ThreadPool.QueueUserWorkItem(state =>
            {
                try
                {
                    var ret = fun(arg1, arg2, arg3);
                    GlobalVarPool.Ins.LogicService.Perform(() => tcs.SetResult(ret));
                }
                catch (Exception ex)
                {
                    GlobalVarPool.Ins.LogicService.Perform(() => tcs.SetException(ex));
                }
            });

            return await tcs.Task;
        }

        #endregion

        #region 异步执行并以回调形式返回结果
        public static void ExcuteTaskAndReturnByCallback<TResult>(Func<Task<TResult>> fun, Action<TResult> cb)
        {
            ThreadPool.QueueUserWorkItem(async state =>
            {
                try
                {
                    var ret = await fun();
                    GlobalVarPool.Ins.LogicService.Perform(() => cb(ret));
                }
                catch (Exception ex)
                {
                    Logger.Ins.Error("ExcuteTaskAndReturnByCallback 失败，{0}:{1}", ex.Message, ex.StackTrace);
                    GlobalVarPool.Ins.LogicService.Perform(() => cb(default(TResult)));
                }
            });
        }

        public static void ExcuteTaskAndReturnByCallback<T1, TResult>(T1 arg1,
            Func<T1, Task<TResult>> fun, Action<TResult> cb)
        {
            ThreadPool.QueueUserWorkItem(async state =>
            {
                try
                {
                    var ret = await fun(arg1);
                    GlobalVarPool.Ins.LogicService.Perform(() => cb(ret));
                }
                catch (Exception ex)
                {
                    Logger.Ins.Error("ExcuteTaskAndReturnByCallback 失败，{0}:{1}", ex.Message, ex.StackTrace);
                    GlobalVarPool.Ins.LogicService.Perform(() => cb(default(TResult)));
                }
            });
        }

        public static void ExcuteTaskAndReturnByCallback<T1, T2, TResult>(T1 arg1, T2 arg2,
            Func<T1, T2, Task<TResult>> fun, Action<TResult> cb)
        {
            ThreadPool.QueueUserWorkItem(async state =>
            {
                try
                {
                    var ret = await fun(arg1, arg2);
                    GlobalVarPool.Ins.LogicService.Perform(() => cb(ret));
                }
                catch (Exception ex)
                {
                    Logger.Ins.Error("ExcuteTaskAndReturnByCallback 失败，{0}:{1}", ex.Message, ex.StackTrace);
                    GlobalVarPool.Ins.LogicService.Perform(() => cb(default(TResult)));
                }
            });
        }
        #endregion
    }
}

#endif