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
                var ret = await Request((short)RpcRoute.GmCmd, new Message2Server { Message = "Tester request" });

                if (ret.Item1)
                {
                    continue;
                }

                Logger.Instance.Error("Server response false");
            }
        }

        public override object HandleRequest(short route, byte[] param)
        {
            // 该客户端无视任何请求
            return null;
        }

        public override bool HandleNotify(short route, byte[] param)
        {
            // 该客户端无视任何通知
            return true;
        }
    }
}