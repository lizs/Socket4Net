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
using ecs;
using Shared;
using socket4net;

namespace Sample
{
    public class SampleComponent : SampleComponentBase
    {
        public async override Task<RpcResult> OnRequest(short ops, byte[] data)
        {
            switch ((EOps)ops)
            {
                case EOps.Echo:
                {
                    var proto = PiSerializer.Deserialize<EchoProto>(data);
                    return
                        RpcResult.MakeSuccess(new EchoProto
                        {
                            Message = string.Format("Response from server : {0}", proto.Message)
                        });
                }

                case EOps.Create:
                {
                    var player = GetAncestor<Player>();
                    player.Es.CreateDefault<Ship>(new EntityArg(this, Uid.New()), true);
                    return true;
                }

                case EOps.Destroy:
                {
                    var player = GetAncestor<Player>();
                    player.Es.DestroyAll();
                    return true;
                }

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

                        // 广播该消息至客户端ChatComponent
                        ServerNodesMgr.Ins.GetServer<SampleSession>()
                            .Broadcast(0, (short) EOps.Broadcst, new BroadcastProto
                            {
                                Message = Host.Name + " : " + proto.Message
                            }, Id);
                        return true;
                    }

                default:
                    return false;
            }
        }
    }
}
