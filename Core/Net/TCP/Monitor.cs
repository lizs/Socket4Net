using System;
using Core.Log;
using Core.Net.TCP;
using Core.RPC;
using Core.Service;

namespace Core.Tool
{
    public class Monitor<TSession, TLogicService, TNetService>
        where TSession : class, ISession, new()
        where TNetService : class ,IService, new()
        where TLogicService : class ,ILogicService, new()
    {
        private Timer.Timer _timer;

        public IPeer<TSession, TLogicService, TNetService> Target { get; private set; }
        public void Start(IPeer<TSession, TLogicService, TNetService> target, int delay = 1000, int period = 5 * 1000)
        {
            Target = target;
            _timer = new Timer.Timer(target.LogicService,  "PerformanceCounter for " + target.EndPoint, 1000, 5 * 1000);
            _timer.Start();
            _timer.Arrived += OutputPerformance;
        }

        public void Stop()
        {
            _timer.Arrived -= OutputPerformance;
            _timer.Stop();

            Target = null;
        }

        private void OutputPerformance(Timer.Timer timer)
        {
            Logger.Instance.InfoFormat("Logic Jobs : {0}, Net jobs : {1}, Sessions :  {2}", Target.LogicService.Jobs, Target.NetService.Jobs, Target.SessionMgr.Count);
            GC.Collect();
        }
    }

    public class Monitor : Monitor<RpcSession, LogicService, NetService>
    {
    }
}