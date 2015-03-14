
using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;

namespace Core.Net.TCP
{
    internal class SendingItem
    {
        public ISession Session { get; set; }
        public byte[] Data { get; set; }
    }

    public static class SendingSevice
    {
        private static readonly Thread Worker = new Thread(WorkerProcedure);
        private static readonly ConcurrentQueue<SendingItem> Jobs = new ConcurrentQueue<SendingItem>();
        public static int SendingQueueCount { get { return Jobs.Count; } }

        private static SocketAsyncEventArgs _sendAsyncEventArgs;
        private static AutoResetEvent _sendEvent;

        private static bool Busy { get { return _busy == 1; } }
        private static int _busy;

        public static void Shutdown()
        {
            _sendAsyncEventArgs.Dispose();
            _sendEvent.Dispose();

            Worker.Join();
        }

        public static void Startup()
        {
            _sendAsyncEventArgs = new SocketAsyncEventArgs();
            _sendAsyncEventArgs.Completed += OnSendCompleted;

            _sendEvent = new AutoResetEvent(false);

            Worker.Start();
        }

        public static void Push(ISession session, byte[] data)
        {
            Jobs.Enqueue(new SendingItem{Session = session, Data = data});
            _sendEvent.Set();
        }

        private static void WorkerProcedure()
        {
            while (true)
            {
                _sendEvent.WaitOne();

                if (Busy) continue;
                SendingItem item;
                if (!Jobs.TryDequeue(out item)) continue;
                if (item.Session.Closed) continue;

                Send(item);
            }
        }

        private static void Send(SendingItem item)
        {
            try
            {
                _sendAsyncEventArgs.SetBuffer(item.Data, 0, item.Data.Length);
                _sendAsyncEventArgs.UserToken = item;
                if (item.Session.UnderlineSocket.SendAsync(_sendAsyncEventArgs))
                    MarkAsBusy();
                else
                    MarkAsFree();
            }
            catch (ObjectDisposedException e)
            {
                NetLogger.Log.Warn("Socket already closed!");
                throw;
            }
            catch
            {
                NetLogger.Log.Warn("Exception catch in send!");
                item.Session.Close(SessionCloseReason.WriteError);
                MarkAsFree();
            }
        }

        private static void OnSendCompleted(object sender, SocketAsyncEventArgs e)
        {
            var item = (SendingItem)e.UserToken;
            if (e.SocketError != SocketError.Success)
            {
                item.Session.Close(SessionCloseReason.WriteError);
            }

            MarkAsFree();
        }

        private static void MarkAsBusy()
        {
            Interlocked.CompareExchange(ref _busy, 1, 0);
            NetLogger.Log.Info("Mark busy!");
        }

        private static void MarkAsFree()
        {
            Interlocked.CompareExchange(ref _busy, 0, 1);
            NetLogger.Log.Info("Mark free!");

            _sendEvent.Set();
        }
    }
}
