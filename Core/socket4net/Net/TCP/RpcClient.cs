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

namespace socket4net
{
    public class RpcClient<TSession> : Client<TSession>
        where TSession : class, IRpcSession, new()
    {
#if NET45
        public async Task<RpcResult> RequestAsync<T>(byte targetServer, long playerId, short ops, T proto, long objId,
            short componentId)
        {
            var session = Session;
            if (session == null) return false;
            return await session.RequestAsync(targetServer, playerId, ops, proto, objId, componentId);
        }

        public async Task<RpcResult> RequestAsync(byte targetServer, long playerId, short ops, byte[] data, long objId,
            short componentId)
        {
            var session = Session;
            if (session == null) return false;
            return await session.RequestAsync(targetServer, playerId, ops, data, objId, componentId);
        }

        public async Task<RpcResult> RequestAsync(byte targetServer, long playerId, short ops, long objId, short componentId)
        {
            var session = Session;
            if (session == null) return false;
            return await session.RequestAsync(targetServer, playerId, ops, null, objId, componentId);
        }
#endif

        public void RequestAsync<T>(byte targetServer, long playerId, short ops, T proto, long objId, short componentId,
            Action<bool, byte[]> cb)
        {
            var session = Session;
            if (session == null)
            {
                if (cb != null)
                    cb(false, null);

                return;
            }

            session.RequestAsync(targetServer, playerId, ops, proto, objId, componentId, cb);
        }

        public void RequestAsync(byte targetServer, long playerId, short ops, byte[] data, long objId, short componentId,
            Action<bool, byte[]> cb)
        {
            var session = Session;
            if (session == null)
            {
                if (cb != null)
                    cb(false, null);

                return;
            }

            session.RequestAsync(targetServer, playerId, ops, data, objId, componentId, cb);
        }

        public void RequestAsync(byte targetServer, long playerId, short ops, long objId, short componentId,
            Action<bool, byte[]> cb)
        {
            RequestAsync(targetServer, playerId, ops, null, objId, componentId, cb);
        }

        public void Push<T>(byte targetServer, long playerId, short ops, T proto, long objId, short componentId)
        {
            var session = Session;
            if (session == null)
                return;

            session.Push(targetServer, playerId, ops, proto, objId, componentId);
        }

        public void Push(byte targetServer, long playerId, short ops, byte[] data, long objId, short componentId)
        {
            var session = Session;
            if (session == null)
                return;

            session.Push(targetServer, playerId, ops, data, objId, componentId);
        }

        public void Push<T>(long playerId, short ops, T proto, long objId, short componentId)
        {
            Push(0, playerId, ops, proto, objId, componentId);
        }

        public void Push(long playerId, short ops, byte[] data, long objId, short componentId)
        {
            Push(0, playerId, ops, data, objId, componentId);
        }
    }
}