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
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
#if NET45
using System.Threading.Tasks;
#endif

namespace socket4net
{
    public class SessionArg : UniqueObjArg<long>
    {
        public SessionArg(IPeer owner, long key, Socket underlineSock) : base(owner, key)
        {
            UnderlineSocket = underlineSock;
        }

        public Socket UnderlineSocket { get; private set; }
    }

    public abstract class Session : UniqueObj<long>, ISession
    {
        public const ushort DefaultPackageMaxSize = 4 * 1024;
        public const ushort DefaultReceiveBufferSize = 4 * 1024;
        protected const ushort HeaderSize = sizeof(ushort);

        public override string Name
        {
            get { return string.Format("{0}:{1}", GetType().Name, Id); }
        }

        public Socket UnderlineSocket { get; private set; }
        public IPeer HostPeer { get { return Owner as IPeer; } }

        /// <summary>
        /// 指定接收buffer长度
        /// </summary>
        public ushort ReceiveBufSize
        {
            get { return _receiveBufferSize; }
            protected set { _receiveBufferSize = value; }
        }

        /// <summary>
        /// 限制包大小
        /// </summary>
        public ushort PackageMaxSize
        {
            get { return _packageMaxSize; }
            protected set { _packageMaxSize = value; }
        }

        // 发送相关
        private bool _isSending;
        private Queue<byte[]> _sendingQueue;
        private SocketAsyncEventArgs _sendAsyncEventArgs;

        // 接收相关
        private SocketAsyncEventArgs _receiveAsyncEventArgs;
        private CircularBuffer _receiveBuffer;
        private Packer _packer;

        private bool _closed;
        private ushort _receiveBufferSize = DefaultReceiveBufferSize;
        private ushort _packageMaxSize = DefaultPackageMaxSize;

        protected override void OnInit(ObjArg arg)
        {
            base.OnInit(arg);

            var more = arg.As<SessionArg>();
            UnderlineSocket = more.UnderlineSocket;
            _sendingQueue = new Queue<byte[]>();

            _sendAsyncEventArgs = new SocketAsyncEventArgs();
            _sendAsyncEventArgs.Completed += OnSendCompleted;

            _receiveAsyncEventArgs = new SocketAsyncEventArgs();
            _receiveAsyncEventArgs.Completed += OnReceiveCompleted;
        }

        /// <summary>
        /// 分包
        /// 在LogicService线程分发
        /// </summary>
        /// <param name="data"></param>
#if NET35
        public abstract void Dispatch(byte[] data);
#else
        public abstract Task Dispatch(byte[] data);
#endif

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Close(SessionCloseReason.ClosedByMyself);
        }

        public virtual void Close(SessionCloseReason reason)
        {
            if (_closed) return;
            _closed = true;

            _sendingQueue.Clear();

            if (UnderlineSocket.Connected)
                UnderlineSocket.Shutdown(SocketShutdown.Both);

            UnderlineSocket.Close();
            UnderlineSocket = null;

            _sendAsyncEventArgs.Dispose();
            _receiveAsyncEventArgs.Dispose();
            _sendAsyncEventArgs = null;
            _receiveAsyncEventArgs = null;
            _receiveBuffer = null;
            _packer = null;

            HostPeer.SessionMgr.RemoveSession(Id, reason);
        }

        protected byte[][] Split(byte[] data, ushort maxLen)
        {
            if (data == null) return new byte[][] { };
            if (data.Length <= maxLen) return new[] { data };

            var segmentsCnt = data.Length / maxLen;
            var tailSize = data.Length % maxLen;
            if (tailSize != 0)
                ++segmentsCnt;

            var segments = new byte[segmentsCnt][];
            for (var i = 0; i < segmentsCnt - 1; ++i)
            {
                segments[i] = new byte[maxLen];
                Buffer.BlockCopy(data, i * maxLen, segments[i], 0, maxLen);
            }

            // 尾包
            segments[segmentsCnt - 1] = new byte[tailSize];
            Buffer.BlockCopy(data, (segmentsCnt - 1) * maxLen, segments[segmentsCnt - 1], 0, tailSize);

            return segments;
        }

        public void InternalSend(NetPackage pack)
        {
            Send(PiSerializer.Serialize(pack));
        }

        private void Send(byte[] data)
        {
            if (_closed || data == null || data.Length == 0) return;

            var segments = Split(data, PackageMaxSize);

            // 头包
            Send(segments[0], (byte)segments.Length);

            // 中间包
            for (var i = 1; i < segments.Length; i++)
                Send(segments[i], 1);
        }

        private void Send(byte[] data, byte segmentsCnt)
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Write((ushort)data.Length);
                bw.Write(segmentsCnt);
                bw.Write(data);

                HostPeer.PerformInNet(SendImp, ms.ToArray());
            }
        }

        private void SendImp(byte[] data)
        {
            if (_closed) return;

            if (_isSending)
            {
                _sendingQueue.Enqueue(data);
            }
            else
            {
                SendAsync(data);
            }
        }

        private void SendNext()
        {
            if (_closed) return;

            if (_sendingQueue.Count > 0)
            {
                var data = _sendingQueue.Dequeue();
                SendAsync(data);
            }
        }

        private void SendAsync(byte[] data)
        {
            try
            {
                _sendAsyncEventArgs.SetBuffer(data, 0, data.Length);
                _isSending = UnderlineSocket.SendAsync(_sendAsyncEventArgs);
            }
            catch (ObjectDisposedException e)
            {
                Logger.Ins.Warn("Socket already closed! detail : {0}", e.StackTrace);
            }
            catch
            {
                Close(SessionCloseReason.WriteError);
            }

            if (!_isSending)
            {
                GlobalVarPool.Ins.NetService.OnWriteCompleted(data.Length);
                SendNext();
            }
        }

        private void OnSendCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (_closed) return;

            HostPeer.PerformInNet(() =>
            {
                if (e.SocketError != SocketError.Success)
                {
                    Close(SessionCloseReason.WriteError);
                    return;
                }

                GlobalVarPool.Ins.NetService.OnWriteCompleted(e.BytesTransferred);

                if (_sendingQueue.Count > 0)
                    SendNext();
                else
                    _isSending = false;
            });
        }

        private void WakeupReceive()
        {
            ReceiveNext();
        }

        protected override void OnStart()
        {
            base.OnStart();
            _receiveBuffer = new CircularBuffer(ReceiveBufSize);
            _packer = new Packer(PackageMaxSize);

            // 投递首次接受请求
            WakeupReceive();
        }

        private void ProcessReceive()
        {
            if (_closed) return;

            if (_receiveAsyncEventArgs.SocketError != SocketError.Success)
            {
                Close(SessionCloseReason.ReadError);
                return;
            }

            if (_receiveAsyncEventArgs.BytesTransferred == 0)
            {
                Close(SessionCloseReason.ClosedByRemotePeer);
                return;
            }

            _receiveBuffer.MoveByWrite((ushort)_receiveAsyncEventArgs.BytesTransferred);
            ushort packagesCnt = 0;
            if (_packer.Process(_receiveBuffer, ref packagesCnt) == PackerError.Failed)
            {
                Close(SessionCloseReason.PackError);
                return;
            }

            if (_receiveBuffer.Overload)
                _receiveBuffer.Reset();

            GlobalVarPool.Ins.NetService.OnReadCompleted(_receiveAsyncEventArgs.BytesTransferred, packagesCnt);

            while (_packer.Packages.Count > 0)
            {
                Dispatch();
            }

            ReceiveNext();
        }

        private void Dispatch()
        {
            var pack = _packer.Packages.Dequeue();
            HostPeer.PerformInLogic(() => Dispatch(pack));
        }

        private void ReceiveNext()
        {
            if (_closed) return;

            _receiveAsyncEventArgs.SetBuffer(_receiveBuffer.Buffer, _receiveBuffer.Tail, _receiveBuffer.WritableSize);
            _receiveAsyncEventArgs.UserToken = _receiveBuffer;
            if (!UnderlineSocket.ReceiveAsync(_receiveAsyncEventArgs))
            {
                ProcessReceive();
            }
        }

        private void OnReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (_closed) return;

            HostPeer.PerformInNet(() =>
            {
                if (e.SocketError != SocketError.Success)
                {
                    Close(SessionCloseReason.ReadError);
                    return;
                }

                ProcessReceive();
            });
        }
    }
}
