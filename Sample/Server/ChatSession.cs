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
using System.Security.Cryptography;
using System.Text;
using Proto;
using socket4net;
#if NET45
using System.Threading.Tasks;
#endif

namespace Sample
{
    public class ChatSession : DispatchableSession
    {
        private DES _des;
        private readonly byte[] _desKey = Encoding.Default.GetBytes("12345678");
        protected override void OnInit(ObjArg arg)
        {
            base.OnInit(arg);

            // 设置加密、解密方法
            _des = DES.Create();

            var encryptor = _des.CreateEncryptor(_desKey, _desKey);
            Encoder = bytes => encryptor.TransformFinalBlock(bytes, 0, bytes.Length);

            var decryptor = _des.CreateDecryptor(_desKey, _desKey);
            Decoder = bytes => decryptor.TransformFinalBlock(bytes, 0, bytes.Length);
        }

#if NET45
        public override Task<NetResult> HandleRequest(IDataProtocol rq)
        {
            var more = rq as DefaultDataProtocol;

            switch ((EOps) more.Ops)
            {
                case EOps.Reqeust:
                {
                    var proto = PiSerializer.Deserialize<RequestProto>(more.Data);
                    return Task.FromResult(NetResult.MakeSuccess(new ResponseProto
                    {
                        Message = string.Format("Response from server : {0}", proto.Message)
                    }));
                }
            }

            return null;
        }

        public override Task<bool> HandlePush(IDataProtocol ps)
        {
            var more = ps as DefaultDataProtocol;
            switch ((EOps) more.Ops)
            {
                case EOps.Push:
                {
                    var proto = PiSerializer.Deserialize<PushProto>(more.Data);

                    // 广播例子
                    Broadcast(new DefaultDataProtocol
                    {
                        Ops = (short) EOps.Push,
                        Data = PiSerializer.Serialize(new PushProto
                        {
                            Message = Name + " : " + proto.Message
                        })
                    });

                    return Task.FromResult(true);
                }
            }

            return Task.FromResult(false);
        }
#else
        public override void HandleRequest(IDataProtocol rq, Action<NetResult> cb)
        {
            var more = rq as DefaultDataProtocol;

            switch ((EOps) more.Ops)
            {
                case EOps.Reqeust:
                {
                    var proto = PiSerializer.Deserialize<RequestProto>(more.Data);
                    cb(NetResult.MakeSuccess(new ResponseProto
                    {
                        Message = string.Format("Response from server : {0}", proto.Message)
                    }));
                    break;
                }
            }
        }

        public override void HandlePush(IDataProtocol ps, Action<bool> cb)
        {
            var more = ps as DefaultDataProtocol;
            switch ((EOps)more.Ops)
            {
                case EOps.Push:
                    {
                        var proto = PiSerializer.Deserialize<PushProto>(more.Data);

                        // 广播例子
                        Broadcast(new DefaultDataProtocol
                        {
                            Ops = (short)EOps.Push,
                            Data = PiSerializer.Serialize(new PushProto
                            {
                                Message = Name + " : " + proto.Message
                            })
                        });

                        cb(true);
                        return;
                    }
            }

            cb(false);
        }
#endif
    }
}
