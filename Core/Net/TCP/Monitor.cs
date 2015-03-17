using System;
using System.Text;
using Core.Log;
using Core.RPC;
using Core.Service;

namespace Core.Net.TCP
{
    public class Monitor<TSession, TLogicService, TNetService>
        where TSession : class, ISession, new()
        where TNetService : class ,INetService, new()
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
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Logic Jobs : {0}, Net jobs : {1}, Sessions :  {2}", Target.LogicService.Jobs,
                Target.NetService.Jobs, Target.SessionMgr.Count);
            sb.AppendLine();
            sb.AppendFormat("Write : {0}KB/s, Read : {1}KB/s, Write : {2}/s, Read : {3}/s",
                Target.NetService.WriteBytesPerSec / 1024.0f, Target.NetService.ReadBytesPerSec / 1024.0f,
                Target.NetService.WritePackagesPerSec, Target.NetService.ReadPackagesPerSec);
            sb.AppendLine();
            sb.AppendFormat("LJob : {0}/s, NJob : {1}/s", Target.LogicService.ExcutedJobsPerSec, Target.NetService.ExcutedJobsPerSec);

            Logger.Instance.Info(sb.ToString());
            GC.Collect();
        }
    }

    public class Monitor : Monitor<RpcSession, LogicService, NetService>
    {
    }
}