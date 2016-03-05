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
using System.Net;

namespace socket4net
{
    /// <summary>
    /// 终端
    /// 对服务器、客户端的抽象
    /// </summary>
    public interface IPeer : IObj
    {
        string Ip { get; }
        ushort Port { get; }
        IPAddress Address { get; }
        EndPoint EndPoint { get; }

        SessionMgr SessionMgr { get; }
        
        ILogicService LogicService { get; }
        INetService NetService { get; }

        event Action<ISession, SessionCloseReason> EventSessionClosed;
        event Action<ISession> EventSessionEstablished;
        event Action<string> EventErrorCatched;
        event Action EventPeerClosing;
        
        void PerformInLogic(Action action);
        void PerformInLogic<TParam>(Action<TParam> action, TParam param);

        void PerformInNet(Action action);
        void PerformInNet<TParam>(Action<TParam> action, TParam param);
    }
}