using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using Core.Serialize;
using Core.Service;

namespace Core.Net.TCP
{
    public enum SessionCloseReason
    {
        ClosedByMyself,
        ClosedByRemotePeer,
        ReadError,
        WriteError,
        PackError,
    }

    public interface ISession
    {
        long Id { get; }
        Socket UnderlineSocket { get; set; }
        int SendingQueueCount { get; }
        void Start();
        void Close(SessionCloseReason reason);
        void SendWithHeader(byte[] data);
        void Send(byte[] data);
        void Send<T>(T proto);
        void Dispatch(byte[] pack);
    }

    public abstract class Session : ISession
    {
        public Socket UnderlineSocket { get; set; }
        public long Id { get; set; }

        // 发送相关
        public int SendingQueueCount { get { return _sendingQueue.Count; } }
        private bool _isSending;
        private readonly Queue<byte[]> _sendingQueue;
        private SocketAsyncEventArgs _sendAsyncEventArgs;

        // 接收相关
        public const int BufferLen = 10 * 1024;
        private SocketAsyncEventArgs _receiveAsyncEventArgs;
        private CircularBuffer _receiveBuffer = new CircularBuffer(BufferLen);
        public Packer Packer { get; set; }

        private bool _closed;

#if DEBUG
        public static long SendCnt = 0;
        public static long ReceiveCnt = 0;
#endif

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
        public abstract void Dispatch(byte[] pack);
        
        public virtual void Close(SessionCloseReason reason)
        {
            if (_closed) return;
            _closed = true;
            
            NetService.Perform(() =>
            {
                _sendAsyncEventArgs.Dispose();
                _receiveAsyncEventArgs.Dispose();

                _sendAsyncEventArgs = null;
                _receiveAsyncEventArgs = null;
                _receiveBuffer = null;

                _sendingQueue.Clear();

                UnderlineSocket.Close();
                UnderlineSocket = null;

                SessionMgr.Remove(Id, reason);
            });
        }

        public void Send(byte[] data)
        {
            if (_closed) return;

            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Write((short)data.Length);
                bw.Write(data);

                NetService.Perform(SendImp, ms.ToArray());
            }
        }

        public void Send<T>(T proto)
        {
            if (_closed) return;

            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                var data = Serializer.Serialize(proto);
                bw.Write((short)data.Length);
                bw.Write(data);

                NetService.Perform(SendImp, ms.ToArray());
            }
        }

        public void SendWithHeader(byte[] data)
        {
            if (_closed) return;
            NetService.Perform(SendImp, data);

#if DEBUG
            Interlocked.Increment(ref SendCnt);
#endif
        }

        public static void Broadcast(byte[] data)
        {
            foreach (var session in SessionMgr.Sessions)
            {
                session.Send(data);
            }
        }

        public static void BroadcastWithHeader(byte[] data)
        {
            foreach (var session in SessionMgr.Sessions)
            {
                session.SendWithHeader(data);
            }
        }

        public static void Broadcast<T>(T proto)
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                var data = Serializer.Serialize(proto);
                bw.Write((short)data.Length);
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
                try
                {
                    _sendAsyncEventArgs.SetBuffer(data, 0, data.Length);
                    _isSending = UnderlineSocket.SendAsync(_sendAsyncEventArgs);
                }
                catch (ObjectDisposedException e)
                {
                    NetLogger.Log.Warn("Socket already closed!");
                }
                catch
                {
                    Close(SessionCloseReason.WriteError);
                }
            }
        }

        private void SendNext()
        {
            if (_closed) return;

            if (_sendingQueue.Count > 0)
            {
                var data = _sendingQueue.Dequeue();

                try
                {
                    _sendAsyncEventArgs.SetBuffer(data, 0, data.Length);
                    _isSending = UnderlineSocket.SendAsync(_sendAsyncEventArgs);
                }
                catch (ObjectDisposedException e)
                {
                    NetLogger.Log.Warn("Socket already closed!");
                }
                catch
                {
                    Close(SessionCloseReason.WriteError);
                }

                if (!_isSending)
                    SendNext();
            }
        }

        private void WakeupReceive()
        {
            ReceiveNext();
        }

        public void Start()
        {
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

            _receiveBuffer.MoveByWrite((short)_receiveAsyncEventArgs.BytesTransferred);
            if (Packer.Process(_receiveBuffer) == PackerError.Failed)
            {
                Close(SessionCloseReason.PackError);
                return;
            }

            if (_receiveBuffer.Overload)
                _receiveBuffer.Reset();

            while (Packer.Packages.Count > 0)
            {
                Dispatch();
            }

            ReceiveNext();
        }

        private void Dispatch()
        {
            var pack = Packer.Packages.Dequeue();
            StaService.Perform(() => Dispatch(pack));
#if DEBUG
            Interlocked.Increment(ref ReceiveCnt);
#endif
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

        private void OnSendCompleted(object sender, SocketAsyncEventArgs e)
        {
            NetService.Perform(() =>
            {
                if (e.SocketError != SocketError.Success)
                {
                    Close(SessionCloseReason.WriteError);
                    return;
                }

                if (_sendingQueue.Count > 0)
                    SendNext();
                else
                    _isSending = false;
            });
        }

        private void OnReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            NetService.Perform(() =>
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
