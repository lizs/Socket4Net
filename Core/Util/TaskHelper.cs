

using System;
using System.Threading.Tasks;

namespace socket4net.Util
{
    internal class TaskHelper
    {
//         public static Action<bool, byte[]> Finish<T>(TaskCompletionSource<T> tcs, Action callback)
//         {
//             return (b, bytes) =>
//             {
//                 if (!tcs.TrySetResult(new Tuple<bool, byte[]>(b, bytes)))
//                 {
//                     tcs.SetResult(new Tuple<bool, byte[]>(false, null));
//                 }
//             };
//         }


        //public static Action<Exception, T> Finish<T>(TaskCompletionSource<T> tcs, Action callback)
        //{
        //    var finished = false;

        //    return (exception, value) =>
        //    {
        //        if (finished) return;
        //        finished = true;

        //        if (callback != null) callback();

        //        if (exception != null)
        //            tcs.SetException(exception);
        //        else
        //            tcs.SetResult(value);
        //    };
        //}

        //public static Action<Exception> Exception(TaskCompletionSource<object> tcs)
        //{
        //    return exception =>
        //    {
        //        if (exception != null)
        //        {
        //            tcs.SetException(exception);
        //        }
        //        else
        //        {
        //            tcs.SetResult(null);
        //        }
        //    };
        //}

        //public static Action<Exception, TResult> Exception<TResult>(TaskCompletionSource<TResult> tcs)
        //{
        //    return (exception, result) =>
        //    {
        //        if (exception != null)
        //        {
        //            tcs.SetException(exception);
        //        }
        //        else
        //        {
        //            tcs.SetResult(result);
        //        }
        //    };
        //}

        public static Task<T> Wrap<T>(Action action)
        {
            var tcs = new TaskCompletionSource<T>();
            try
            {
                action();
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }

            return tcs.Task;
        }

        public static Task Wrap(Action action)
        {
            var tcs = new TaskCompletionSource<object>();
            try
            {
                action();
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
            return tcs.Task;
        }
// 
//         public static Task Wrap(Action<Action<Exception>> action)
//         {
//             var tcs = new TaskCompletionSource<object>();
//             try
//             {
//                 action(Exception(tcs));
//                 SetStatus(tcs.Task, TaskStatus.Running);
//             }
//             catch (Exception ex)
//             {
//                 tcs.SetException(ex);
//             }
//             return tcs.Task;
//         }
// 
//         public static Task Wrap<T>(T arg1, Action<T, Action<Exception>> action)
//         {
//             var tcs = new TaskCompletionSource<object>();
//             try
//             {
//                 action(arg1, Exception(tcs));
//                 SetStatus(tcs.Task, TaskStatus.Running);
//             }
//             catch (Exception ex)
//             {
//                 tcs.SetException(ex);
//             }
//             return tcs.Task;
//         }
// 
//         public static Task Wrap<T1, T2>(T1 arg1, T2 arg2, Action<T1, T2, Action<Exception>> action)
//         {
//             var tcs = new TaskCompletionSource<object>();
//             try
//             {
//                 action(arg1, arg2, Exception(tcs));
//                 SetStatus(tcs.Task, TaskStatus.Running);
//             }
//             catch (Exception ex)
//             {
//                 tcs.SetException(ex);
//             }
//             return tcs.Task;
//         }
// 
//         public static Task Wrap<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3, Action<T1, T2, T3, Action<Exception>> action)
//         {
//             var tcs = new TaskCompletionSource<object>();
//             try
//             {
//                 action(arg1, arg2, arg3, Exception(tcs));
//                 SetStatus(tcs.Task, TaskStatus.Running);
//             }
//             catch (Exception ex)
//             {
//                 tcs.SetException(ex);
//             }
//             return tcs.Task;
//         }
// 
//         public static Task Wrap<T1, T2, T3, T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4,
//             Action<T1, T2, T3, T4, Action<Exception>> action)
//         {
//             var tcs = new TaskCompletionSource<object>();
//             try
//             {
//                 action(arg1, arg2, arg3, arg4, Exception(tcs));
//                 SetStatus(tcs.Task, TaskStatus.Running);
//             }
//             catch (Exception ex)
//             {
//                 tcs.SetException(ex);
//             }
//             return tcs.Task;
//         }
// 
//         public static Task<TResult> Wrap<T, TResult>(T arg1, Func<T, Action<Exception>, TResult> func)
//         {
//             var tcs = new TaskCompletionSource<TResult>();
//             try
//             {
//                 var res = default(TResult);
//                 res = func(arg1, (ex) =>
//                 {
//                     if (ex == null)
//                     {
//                         tcs.SetResult(res);
//                     }
//                     else
//                     {
//                         tcs.SetException(ex);
//                     }
//                 });
//                 SetStatus(tcs.Task, TaskStatus.Running);
//             }
//             catch (Exception ex)
//             {
//                 tcs.SetException(ex);
//             }
//             return tcs.Task;
//         }
// 
//         public static Task<TResult> Wrap<T1, T2, TResult>(T1 arg1, T2 arg2,
//             Func<T1, T2, Action<Exception>, TResult> func)
//         {
//             var tcs = new TaskCompletionSource<TResult>();
//             try
//             {
//                 var res = default(TResult);
//                 res = func(arg1, arg2, (ex) =>
//                 {
//                     if (ex == null)
//                     {
//                         tcs.SetResult(res);
//                     }
//                     else
//                     {
//                         tcs.SetException(ex);
//                     }
//                 });
//                 SetStatus(tcs.Task, TaskStatus.Running);
//             }
//             catch (Exception ex)
//             {
//                 tcs.SetException(ex);
//             }
//             return tcs.Task;
//         }
// 
//         public static Task<TResult> Wrap<T, TResult>(T arg1, Action<T, Action<Exception, TResult>> action)
//         {
//             var tcs = new TaskCompletionSource<TResult>();
//             try
//             {
//                 action(arg1, Exception(tcs));
//                 SetStatus(tcs.Task, TaskStatus.Running);
//             }
//             catch (Exception ex)
//             {
//                 tcs.SetException(ex);
//             }
//             return tcs.Task;
//         }
// 
//         public static Task<TResult> Wrap<T1, T2, TResult>(T1 arg1, T2 arg2,
//             Action<T1, T2, Action<Exception, TResult>> action)
//         {
//             var tcs = new TaskCompletionSource<TResult>();
//             try
//             {
//                 action(arg1, arg2, Exception(tcs));
//                 SetStatus(tcs.Task, TaskStatus.Running);
//             }
//             catch (Exception ex)
//             {
//                 tcs.SetException(ex);
//             }
//             return tcs.Task;
//         }
// 
//         public static Task<TResult> Wrap<T1, T2, T3, TResult>(T1 arg1, T2 arg2, T3 arg3,
//             Action<T1, T2, T3, Action<Exception, TResult>> action)
//         {
//             var tcs = new TaskCompletionSource<TResult>();
//             try
//             {
//                 action(arg1, arg2, arg3, Exception(tcs));
//                 SetStatus(tcs.Task, TaskStatus.Running);
//             }
//             catch (Exception ex)
//             {
//                 tcs.SetException(ex);
//             }
//             return tcs.Task;
//         }
// 
//         public static Task<TResult> Wrap<T1, T2, T3, T4, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4,
//             Action<T1, T2, T3, T4, Action<Exception, TResult>> action)
//         {
//             var tcs = new TaskCompletionSource<TResult>();
//             try
//             {
//                 action(arg1, arg2, arg3, arg4, Exception(tcs));
//                 SetStatus(tcs.Task, TaskStatus.Running);
//             }
//             catch (Exception ex)
//             {
//                 tcs.SetException(ex);
//             }
//             return tcs.Task;
//         }
// 
//         public static Task<TResult> Wrap<T1, T2, T3, T4, T5, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5,
//             Action<T1, T2, T3, T4, T5, Action<Exception, TResult>> action)
//         {
//             var tcs = new TaskCompletionSource<TResult>();
//             try
//             {
//                 action(arg1, arg2, arg3, arg4, arg5, Exception(tcs));
//                 SetStatus(tcs.Task, TaskStatus.Running);
//             }
//             catch (Exception ex)
//             {
//                 tcs.SetException(ex);
//             }
//             return tcs.Task;
//         }
    }
}
