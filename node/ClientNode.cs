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

namespace node
{
    public enum ENodeEvent
    {
        Registered,
    }

    public class ClientNode<TSession> : SingleSessionNode<TSession>, IWatchable<ENodeEvent>
        where TSession : class, IRpcSession, new()
    {
        /// <summary>
        ///     节点watch事件
        /// </summary>
        public event Func<ENodeEvent, bool> Watch;

        private bool _registered;
        public bool Registered
        {
            get { return _registered; }
            protected set
            {
                _registered = value;
                if (Watch != null)
                    Watch(ENodeEvent.Registered);
            }
        }

        public bool AutoReconnectEnabled
        {
            get { return (Config as ClientNodeElement).AutoReconnect; }
        }

        public string Ip
        {
            get { return Config.Ip; }
        }

        public ushort Port
        {
            get { return Config.Port; }
        }

        protected override void OnInit(ObjArg arg)
        {
            base.OnInit(arg);
            Peer = New<Client<TSession>>(new ClientArg(null, Ip, Port, AutoReconnectEnabled), false);
        }

        protected override void OnStart()
        {
            base.OnStart();
            (Peer as Obj).Start();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (Peer == null) return;
            (Peer as Obj).Destroy();
        }
    }
}
