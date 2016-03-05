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
using System.Threading.Tasks;
using Shared;
using socket4net;

namespace Sample
{
    public class SampleComponent : SampleComponentBase
    {
        /// <summary>
        ///     请求创建一个Ship
        /// </summary>
        /// <returns></returns>
        public void Add()
        {
            RequestAsync(0, GetAncestor<Player>().Id, (short)EOps.Create, null, 0, Id, (b, bytes) =>
            {
                if (!b)
                    Logger.Ins.Error("Add failed!");
            });
        }

        /// <summary>
        ///     请求删除所有Ship
        /// </summary>
        /// <returns></returns>
        public void Del()
        {
            RequestAsync(0, GetAncestor<Player>().Id, (short)EOps.Destroy, null, 0, Id, (b, bytes) =>
            {
                if (!b)
                    Logger.Ins.Error("Add failed!");
            });
        } 

        /// <summary>
        ///     请求将msg echo回来
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public void Echo(string msg)
        {
            RequestAsync(0, GetAncestor<Player>().Id, (short) EOps.Echo, new EchoProto {Message = msg},
                0, Id, (b, bytes) =>
                {
                    if (b)
                    {
                        var proto = PiSerializer.Deserialize<EchoProto>(bytes);
                        Logger.Ins.Info(proto.Message);
                    }
                    else
                        Logger.Ins.Error("Echo failed!");
                });
        }

        /// <summary>
        ///     请求将msg广播给所有客户端
        /// </summary>
        /// <param name="msg"></param>
        public void Broadcast(string msg)
        {
            Push(0, GetAncestor<Player>().Id, (short)EOps.Broadcst, new BroadcastProto { Message = msg },
                0, Id);
        }

        public async override Task<RpcResult> OnRequest(short ops, byte[] data)
        {
            switch ((EOps)ops)
            {
                default:
                    return RpcResult.Failure;
            }
        }

        public async override Task<bool> OnPush(short ops, byte[] data)
        {
            switch ((EOps)ops)
            {
                case EOps.Broadcst:
                    {
                        var proto = PiSerializer.Deserialize<BroadcastProto>(data);
                        Logger.Ins.Info(proto.Message);
                        return true;
                    }

                default:
                    return false;
            }
        }
    }
}
