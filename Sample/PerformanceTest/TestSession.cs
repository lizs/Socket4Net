using System;
using System.Threading.Tasks;
using socket4net.Log;
using socket4net.RPC;
using Proto;

namespace PerformanceTest
{
    internal class TestSession : RpcSession
    {
        public TestSession()
        {
            ReceiveBufSize = 10 * 1024;
            PackageMaxSize = 40*1024;
        }

        public async void DoTest()
        {
            while (true)
            {
                var ret = await RequestAsync((ushort)RpcRoute.GmCmd, new Message2Server { Message = "Tester request" });

                if (ret.Item1)
                {
                    continue;
                }

                Logger.Instance.Error("Server response false");
            }
        }

        public async override Task<Tuple<bool, byte[]>> HandleRequest(ushort route, byte[] param)
        {
            // 该客户端无视任何请求
            return null;
        }

        public async override Task<bool> HandlePush(ushort route, byte[] param)
        {
            // 该客户端无视任何通知
            return true;
        }
    }
}