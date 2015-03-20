using System;
using System.Threading.Tasks;
using Core.Log;
using Core.RPC;
using Proto;

namespace PerformanceTest
{
    internal class TestSession : RpcSession
    {
        public async void DoTest()
        {
            while (true)
            {
                var ret = await RequestAsync((short)RpcRoute.GmCmd, new Message2Server { Message = "Tester request" });

                if (ret.Item1)
                {
                    continue;
                }

                Logger.Instance.Error("Server response false");
            }
        }

        public async override Task<Tuple<bool, byte[]>> HandleRequest(short route, byte[] param)
        {
            // 该客户端无视任何请求
            return null;
        }

        public async override Task<bool> HandlePush(short route, byte[] param)
        {
            // 该客户端无视任何通知
            return true;
        }
    }
}