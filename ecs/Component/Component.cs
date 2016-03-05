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
using socket4net;
#if NET45
using System.Threading.Tasks;
#endif

namespace ecs
{
    public class ComponentArg : UniqueObjArg<short>
    {
        public ComponentArg(IObj parent, short key)
            : base(parent, key)
        {
        }
    }

    public interface IComponent : IUniqueObj<short>
    {
    }

    /// <summary>
    ///     组件
    ///     游戏逻辑拆分以组件为单元
    /// </summary>
    public abstract class Component : UniqueObj<short>, IComponent
    {
        /// <summary>
        ///     实体爹
        /// </summary>
        public Entity Host
        {
            get { return GetAncestor<Entity>(); }
        }

        /// <summary>
        ///     获取兄弟组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetComponent<T>() where T : Component
        {
            return GetAncestor<Entity>().GetComponent<T>();
        }

        /// <summary>
        ///     获取兄弟组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cpId"></param>
        /// <returns></returns>
        public T GetComponent<T>(short cpId) where T : Component
        {
            return GetAncestor<Entity>().GetComponent<T>(cpId);
        }

        /// <summary>
        ///     启动
        /// </summary>
        protected override void OnStart()
        {
            base.OnStart();
            OnSubscribe();
        }

        /// <summary>
        ///     卸载
        /// </summary>
        protected override void OnDestroy()
        {
            OnUnsubscribe();
            base.OnDestroy();
        }

        /// <summary>
        ///     订阅
        /// </summary>
        protected virtual void OnSubscribe()
        {
        }

        /// <summary>
        ///     反订阅
        /// </summary>
        protected virtual void OnUnsubscribe()
        {
        }

        /// <summary>
        ///     处理消息
        /// </summary>
        /// <param name="msg"></param>
        public virtual void OnMessage(Message msg)
        {
        }

#if NET45
        public virtual Task<RpcResult> OnRequest(short ops, byte[] data)
        {
            return Task.FromResult(RpcResult.Failure);
        }

        public virtual Task<bool> OnPush(short ops, byte[] data)
        {
            return Task.FromResult(false);
        }
#else
        public virtual void OnRequest(short ops, byte[] data, Action<RpcResult> cb)
        {
            cb(RpcResult.Failure);
        }
        
        public virtual void OnPush(short ops, byte[] data, Action<bool> cb)
        {
            cb(false);
        }
#endif
    }
}