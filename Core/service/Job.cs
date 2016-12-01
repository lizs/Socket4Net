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
using System.Threading.Tasks;

namespace socket4net
{
    /// <summary>
    ///     job
    /// </summary>
    public interface IJob
    {
        /// <summary>
        ///     excute
        /// </summary>
        void Do();
    }

    /// <summary>
    ///     异步任务接口
    /// </summary>
    public interface IAsyncJob : IJob
    {
        /// <summary>
        ///     excute
        /// </summary>
        Task<RpcResult> DoAsync();
    }

    /// <summary>
    /// 
    /// </summary>
    public struct Job : IJob
    {
        private readonly Action _procedure;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="proc"></param>
        public Job(Action proc)
        {
            _procedure = proc;
        }

        /// <summary>
        ///     excute
        /// </summary>
        public void Do()
        {
            _procedure?.Invoke();
        }
    }

    /// <summary>
    ///     job with parameters
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct Job<T> : IJob
    {
        private readonly Action<T> _procedure;
        private readonly T _param;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="proc"></param>
        /// <param name="param"></param>
        public Job(Action<T> proc, T param)
        {
            _procedure = proc;
            _param = param;
        }

        /// <summary>
        ///     excute
        /// </summary>
        public void Do()
        {
            _procedure?.Invoke(_param);
        }
    }

    /// <summary>
    ///     异步任务
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct AsyncJob : IAsyncJob
    {
        private readonly Func<Task<RpcResult>> _procedure;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="proc"></param>
        /// <param name="param"></param>
        public AsyncJob(Func<Task<RpcResult>> proc)
        {
            _procedure = proc;
        }

        /// <summary>
        ///     excute
        /// </summary>
        public void Do()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     异步执行
        /// </summary>
        public async Task<RpcResult> DoAsync()
        {
            if(_procedure == null)
                return RpcResult.Failure;

            return await _procedure.Invoke();
        }
    }
}
