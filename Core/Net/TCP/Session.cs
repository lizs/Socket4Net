using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using socket4net.Log;
using socket4net.Serialize;

namespace socket4net.Net.TCP
{
    public enum SessionCloseReason
    {
        ClosedByMyself,
        ClosedByRemotePeer,
        ReadError,
        WriteError,
        PackError,

        /// <summary>
        /// 比如同一账号使用不同会话时，会顶掉之前会话
        /// </summary>
        Replaced,
    }

    public interface ISession
    {
        IPeer HostPeer { get; set; }
        long Id { get; set; }
        Socket UnderlineSocket { get; set; }
        ushort ReceiveBufSize { get; }
        ushort PackageMaxSize { get; }
        
        void Start();
        void Close(SessionCloseReason reason);
        void SendWithHeader(byte[] data);
        void Send(byte[] data);
        void Send<T>(T proto);

        Task Dispatch(byte[] pack);
    }

    public abstract class Session : ISession
    {
        public const ushort DefaultPackageMaxSize = 4 * 1024;
        public const ushort DefaultReceiveBufferSize = 4 * 1024;
        public long Id { get; set; }
        public Socket UnderlineSocket { get; set; }
        public IPeer HostPeer { get; set; }

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
        private readonly Queue<byte[]> _sendingQueue;
        private SocketAsyncEventArgs _sendAsyncEventArgs;

        // 接收相关
        private SocketAsyncEventArgs _receiveAsyncEventArgs;
        private CircularBuffer _receiveBuffer;
        private Packer _packer;

        private bool _closed;
        private ushort _receiveBufferSize = DefaultReceiveBufferSize;
        private ushort _packageMaxSize = DefaultPackageMaxSize;

        protected Session()
        {
            _sendingQueue = new Queue<byte[]>();

            _sendAsyncEventArgs = new SocketAsyncEventArgs();
            _sendAsyncEventArgs.Completed += OnSendCompleted;

            _receiveAsyncEventArgs = new SocketAsyncEventArgs();
            _receiveAsyncEventArgs.Completed += OnReceiveCompleted;
        }

        /// <summary>
        /// 分包
        /// 在STA线程分发
        /// </summary>
        /// <param name="pack"></param>
        public abstract Task Dispatch(byte[] pack);
        
        public virtual void Close(SessionCloseReason reason)
        {
            if (_closed) return;
            _closed = true;
            
            HostPeer.PerformInNet(() =>
            {
                _sendAsyncEventArgs.Dispose();
                _receiveAsyncEventArgs.Dispose();

                _sendAsyncEventArgs = null;
                _receiveAsyncEventArgs = null;
                _receiveBuffer = null;
                _packer = null;

                _sendingQueue.Clear();

                UnderlineSocket.Shutdown(SocketShutdown.Both);
                UnderlineSocket.Close();
                UnderlineSocket = null;

                HostPeer.SessionMgr.Remove(Id, reason);
                HostPeer = null;
            });
        }

        public void Send(byte[] data)
        {
            if (_closed) return;

            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Write((ushort)data.Length);
                bw.Write(data);

                HostPeer.PerformInNet(SendImp, ms.ToArray());
            }
        }

        public void Send<T>(T proto)
        {
            if (_closed) return;

            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                var data = Serializer.Serialize(proto);
                bw.Write((ushort)data.Length);
                bw.Write(data);

                HostPeer.PerformInNet(SendImp, ms.ToArray());
            }
        }

        public void SendWithHeader(byte[] data)
        {
            if (_closed) return;
            HostPeer.PerformInNet(SendImp, data);
        }

        public void Broadcast(byte[] data)
        {
            foreach (var session in HostPeer.SessionMgr.Sessions)
            {
                session.Send(data);
            }
        }

        public void BroadcastWithHeader(byte[] data)
        {
            foreach (var session in HostPeer.SessionMgr.Sessions)
            {
                session.SendWithHeader(data);
            }
        }

        public void Broadcast<T>(T proto)
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                var data = Serializer.Serialize(proto);
                bw.Write((ushort)data.Length);
                bw.Write(data);

                BroadcastWithHeader(ms.ToArray());
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
                Logger.Instance.Warn("Socket already closed!");
            }
            catch
            {
                Close(SessionCloseReason.WriteError);
            }

            if (!_isSending)
            {
                HostPeer.NetService.OnWriteCompleted(data.Length);
                SendNext();
            }
        }

        private void OnSendCompleted(object sender, SocketAsyncEventArgs e)
        {
            if(_closed) return;

            HostPeer.PerformInNet(() =>
            {
                if (e.SocketError != SocketError.Success)
                {
                    Close(SessionCloseReason.WriteError);
                    return;
                }

                HostPeer.NetService.OnWriteCompleted(e.BytesTransferred);

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

        public void Start()
        {
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

            HostPeer.NetService.OnReadCompleted(_receiveAsyncEventArgs.BytesTransferred, packagesCnt);

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
