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
#if NET45
using System.Threading.Tasks;
#endif
using socket4net;

namespace ecs
{
    public abstract class RpcComponent : Component, IRpc
    {
        #region IRpc Members
        public IRpcSession Session
        {
            get { return GetAncestor<Player>().Session; }
        }

#if NET35
        public virtual RpcResult HandleRequest(short ops, byte[] bytes)
        {
            Logger.Ins.Error("Rpc组件 {0} 收到未处理的Request : {1}, ", Name, ops);
            return RpcResult.Failure;
        }

        public virtual bool HandlePush(short ops, byte[] bytes)
        {
            Logger.Ins.Error("Rpc组件 {0} 收到未处理的Push : {1}, ", Name, ops);
            return false;
        }
#else
        public virtual Task<RpcResult> HandleRequest(short ops, byte[] bytes)
        {
            Logger.Ins.Error("Rpc组件 {0} 收到未处理的Request : {1}, ", Name, ops);
            return Task.FromResult(RpcResult.Failure);
        }

        public virtual Task<bool> HandlePush(short ops, byte[] data)
        {
            Logger.Ins.Error("Rpc组件 {0} 收到未处理的Push : {1}, ", Name, ops);
            return Task.FromResult(false);
        }
#endif

        #endregion

        #region 请求对端指定对象、指定组件
#if NET45
        public Task<RpcResult> RequestAsync<T>(byte targetServer, long playerId, short ops, T proto, long objId,
            short cpId)
        {
            return Session.RequestAsync(targetServer, playerId, ops, proto, objId, cpId);
        }

        public Task<RpcResult> RequestAsync(byte targetServer, long playerId, short ops, byte[] data, long objId,
            short cpId)
        {
            return Session.RequestAsync(targetServer, playerId, ops, data, objId, cpId);
        }
#endif

        public void RequestAsync<T>(byte targetServer, long playerId, short ops, T proto, long objId, short cpId,
            Action<bool, byte[]> cb)
        {
            Session.RequestAsync(targetServer, playerId, ops, proto, objId, cpId, cb);
        }

        public void RequestAsync(byte targetServer, long playerId, short ops, byte[] data, long objId, short cpId,
            Action<bool, byte[]> cb)
        {
            Session.RequestAsync(targetServer, playerId, ops, data, objId, cpId, cb);
        }

        #endregion

        #region 推送给对端指定对象的指定组件
        public void Push(byte targetServer, long playerId, short ops, byte[] data, long objId, short cpId)
        {
            Session.Push(targetServer, playerId, ops, data, objId, cpId);
        }

        public void Push<T>(byte targetServer, long playerId, short ops, T proto, long objId, short cpId)
        {
            Session.Push(targetServer, playerId, ops, proto, objId, cpId);
        }
        #endregion
    }
}