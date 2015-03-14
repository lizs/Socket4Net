// 
// using System.Collections.Concurrent;
// using System.Net.Sockets;
// 
// namespace Core.Net.TCP
// {
//     public static class SocketAysncEventArgsPool
//     {
//         public static readonly ConcurrentQueue<SocketAsyncEventArgs> Pool = new ConcurrentQueue<SocketAsyncEventArgs>();
// 
//         public static SocketAsyncEventArgs Get()
//         {
//             SocketAsyncEventArgs ret;
//             return Pool.TryDequeue(out ret) ? ret : new SocketAsyncEventArgs();
//         }
// 
//         public static void Recycle(SocketAsyncEventArgs e)
//         {
//             Pool.Enqueue(e);
//         }
//     }
// }
