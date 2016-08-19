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
using System.Threading;
#if NET45
using System.Threading.Tasks;
#endif

namespace socket4net
{
    /// <summary>
    ///  session arguments
    /// </summary>
    public class SessionArg : UniqueObjArg<long>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="key"></param>
        /// <param name="underlineSock"></param>
        public SessionArg(IPeer owner, long key, Socket underlineSock) : base(owner, key)
        {
            UnderlineSocket = underlineSock;
        }

        /// <summary>
        ///     under line socket associated to the target session
        /// </summary>
        public Socket UnderlineSocket { get; private set; }
    }

    /// <summary>
    ///     Session
    /// </summary>
    public abstract class Session : UniqueObj<long>, ISession
    {
        /// <summary>
        ///     default maximum network package size
        /// </summary>
        public const ushort DefaultPackageMaxSize = 4 * 1024;
        /// <summary>
        ///     default receive buffer size
        /// </summary>
        public const ushort DefaultReceiveBufferSize = 4 * 1024;
        protected const ushort HeaderSize = sizeof(ushort);

        // 默认加密、解密方法（默认不加密）
        protected Func<byte[], byte[]> Encoder = bytes => bytes;
        protected Func<byte[], byte[]> Decoder = bytes => bytes; 

        public override string Name => $"{GetType().Name}:{Id}";

        public Socket UnderlineSocket { get; private set; }

        /// <summary>
        ///     Host peer this session belongs to
        /// </summary>
        public IPeer HostPeer => Owner as IPeer;

        /// <summary>
        /// 指定接收buffer长度
        /// </summary>
        public ushort ReceiveBufSize { get; protected set; } = DefaultReceiveBufferSize;

        /// <summary>
        /// 限制包大小
        /// </summary>
        public ushort PackageMaxSize { get; protected set; } = DefaultPackageMaxSize;

        // 发送相关
        private bool _isSending;
        private Queue<byte[]> _sendingQueue;
        private SocketAsyncEventArgs _sendAsyncEventArgs;

        // 接收相关
        private SocketAsyncEventArgs _receiveAsyncEventArgs;
        private CircularBuffer _receiveBuffer;
        private Packer _packer;

        /// <summary>
        ///     Session already closed
        /// </summary>
        public bool Closed => _closedFlag == 1;

        private int _closedFlag;

        /// <summary>
        ///    internal called when an Obj is initialized
        /// </summary>
        /// <param name="arg"></param>
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

        /// <summary>
        ///    internal called when an Obj is to be destroyed
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();
            Close(SessionCloseReason.ClosedByMyself);
        }

        /// <summary>
        ///     Close this session
        /// </summary>
        /// <param name="reason"></param>
        public virtual void Close(SessionCloseReason reason)
        {
            if (Closed) return;
            Interlocked.Exchange(ref _closedFlag, 1);

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

        private static byte[][] Split(byte[] data, ushort maxLen)
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

        /// <summary>
        ///     Send message to peer
        /// </summary>
        /// <param name="pack"></param>
        public void InternalSend(NetPackage pack)
        {
            Send(PiSerializer.Serialize(pack));
        }

        private void Send(byte[] data)
        {
            if (Closed || data == null || data.Length == 0) return;
            var encoded = Encoder(data);

            var segments = Split(encoded, PackageMaxSize);

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
            if (Closed) return;

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
            if (Closed) return;

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
                return;
            }
            catch
            {
                Close(SessionCloseReason.WriteError);
                return;
            }

            if (!_isSending)
            {
                SendNext();
            }
        }

        private void OnSendCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (Closed) return;

            if (e.SocketError != SocketError.Success)
            {
                Close(SessionCloseReason.WriteError);
                return;
            }

            PerformanceMonitor.Ins.RecordWrite(e.BytesTransferred);
            HostPeer.PerformInNet(() =>
            {
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

        /// <summary>
        ///     Invoked when obj started
        /// </summary>
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
            if (Closed) return;

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

            while (_packer.Packages.Count > 0)
            {
                Dispatch();
            }

            ReceiveNext();
        }

        private void Dispatch()
        {
            var pack = _packer.Packages.Dequeue();
            var decoded = Decoder(pack);
            HostPeer.PerformInLogic(() => Dispatch(decoded));
        }

        private void ReceiveNext()
        {
            if (Closed) return;

            _receiveAsyncEventArgs.SetBuffer(_receiveBuffer.Buffer, _receiveBuffer.Tail, _receiveBuffer.WritableSize);
            _receiveAsyncEventArgs.UserToken = _receiveBuffer;
            if (!UnderlineSocket.ReceiveAsync(_receiveAsyncEventArgs))
            {
                ProcessReceive();
            }
        }

        private void OnReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (Closed) return;

            PerformanceMonitor.Ins.RecordRead(e.BytesTransferred);
            if (e.SocketError != SocketError.Success)
            {
                Close(SessionCloseReason.ReadError);
                return;
            }

            HostPeer.PerformInNet(ProcessReceive);
        }
    }
}
