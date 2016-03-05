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
using ecs;
using node;
using Shared;
using socket4net;

namespace Sample
{
    public class Server : ServerNode<SampleSession>
    {
        public static Server Ins { get; private set; }

        public Server()
        {
            if(Ins != null)
                throw new Exception("Server already instantiated!");

            Ins = this;
        }

        protected override void OnConnected(SampleSession session)
        {
            base.OnConnected(session);

            // 服务器创建玩家
            var player = PlayerMgr.Ins.Create<Player>(
                new FlushablePlayerArg(PlayerMgr.Ins, Uid.New(), session, true,
                    () => RedisMgr<AsyncRedisClient>.Instance.GetFirst(x => x.Config.Type == "Sample")),
                true);

            // 1、通知客户端创建玩家
            session.Push(player.Id, (short)ENonPlayerOps.CreatePlayer, null, 0, 0);

            // 2、下发数据
            player.Flush();
        }

        protected override void OnDisconnected(SampleSession session, SessionCloseReason reason)
        {
            base.OnDisconnected(session, reason);
            PlayerMgr.Ins.Destroy(session.Id);
        }
    }
}
