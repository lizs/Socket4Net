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
using socket4net;

namespace node
{
    public interface ISingleSessionNode : INode
    {
        IRpcSession Session { get; }

        Task<RpcResult> RequestAsync(byte targetNode, long playerId, short ops, byte[] data,
            long objId, short componentId);

        void RequestAsync(byte targetNode, long playerId, short ops, byte[] data, long objId,
            short componentId, Action<bool, byte[]> cb);

        bool Push(byte targetNode, long playerId, short ops, byte[] data, long objId, short componentId);

        Task<RpcResult> RequestAsync<T>(byte targetNode, long playerId, short ops, T proto,
            long objId, short componentId);

        void RequestAsync<T>(byte targetNode, long playerId, short ops, T proto, long objId,
            short componentId, Action<bool, byte[]> cb);

        bool Push<T>(byte targetNode, long playerId, short ops, T proto, long objId, short componentId);
    }

    /// <summary>
    ///     单会话节点
    ///     即：始终仅维护一条会话的节点，一般以客户端居多
    ///     如：Connector2GateClient
    /// </summary>
    public abstract class SingleSessionNode<TSession> : Node<TSession>, ISingleSessionNode where TSession : class, IRpcSession, new()
    {
        private IRpcSession _session;
        public IRpcSession Session
        {
            get { return _session ?? (_session = Peer.SessionMgr.GetFirst<TSession>()); }
        }

        #region 远端

        public async Task<RpcResult> RequestAsync(byte targetNode, long playerId, short ops, byte[] data,
            long objId, short componentId)
        {
            if (Session == null) return RpcResult.Failure;
            return await Session.RequestAsync(targetNode, playerId, ops, data, objId, componentId);
        }

        public void RequestAsync(byte targetNode, long playerId, short ops, byte[] data, long objId,
            short componentId, Action<bool, byte[]> cb)
        {
            if (Session == null)
            {
                cb(false, null);
                return;
            }

            Session.RequestAsync(targetNode, playerId, ops, data, objId, componentId, cb);
        }

        public bool Push(byte targetNode, long playerId, short ops, byte[] data, long objId, short componentId)
        {
            if (Session == null) return false;
            Session.Push(targetNode, playerId, ops, data, objId, componentId);
            return true;
        }

        public async Task<RpcResult> RequestAsync<T>(byte targetNode, long playerId, short ops, T proto,
            long objId, short componentId)
        {
            if (Session == null) return RpcResult.Failure;
            return
                await
                    Session.RequestAsync(targetNode, playerId, ops, PiSerializer.Serialize(proto), objId,
                        componentId);
        }

        public void RequestAsync<T>(byte targetNode, long playerId, short ops, T proto, long objId,
            short componentId, Action<bool, byte[]> cb)
        {
            if (Session == null)
            {
                cb(false, null);
                return;
            }

            Session.RequestAsync(targetNode, playerId, ops, PiSerializer.Serialize(proto), objId, componentId, cb);
        }

        public bool Push<T>(byte targetNode, long playerId, short ops, T proto, long objId, short componentId)
        {
            if (Session == null) return false;
            Session.Push(targetNode, playerId, ops, PiSerializer.Serialize(proto), objId, componentId);
            return true;
        }

        #endregion
    }
}