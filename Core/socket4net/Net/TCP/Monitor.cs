using System;
using System.Text;

namespace socket4net
{
    //public class Monitor : Obj
    //{
    //    private Scheduler _scheduler;

    //    public IPeer Target { get; private set; }

    //    public void Start(IPeer target, int delay = 1000, int period = 5*1000)
    //    {
    //        Target = target;
    //        _scheduler = new Scheduler("PerformanceCounter for " + target.EndPoint);
    //        _scheduler.InvokeRepeating(OutputPerformance, 1000, 5*1000);
    //    }

    //    public void Stop()
    //    {
    //        _scheduler.Destroy();
    //        Target = null;
    //    }

    //    private void OutputPerformance()
    //    {
    //        var sb = new StringBuilder();
    //        sb.AppendFormat("Logic Jobs : {0}, Net jobs : {1}, Sessions :  {2}", Target.LogicService.Jobs,
    //            Target.NetService.Jobs, Target.SessionMgr.Count);
    //        sb.AppendLine();
    //        sb.AppendFormat("Write : {0}KB/s, Read : {1}KB/s, Write : {2}/s, Read : {3}/s",
    //            Target.NetService.WriteBytesPerSec / 1024.0f, Target.NetService.ReadBytesPerSec / 1024.0f,
    //            Target.NetService.WritePackagesPerSec, Target.NetService.ReadPackagesPerSec);
    //        sb.AppendLine();
    //        sb.AppendFormat("LJob : {0}/s, NJob : {1}/s", Target.LogicService.ExcutedJobsPerSec, Target.NetService.ExcutedJobsPerSec);

    //        Logger.Instance.Info(sb.ToString());
    //        GC.Collect();
    //    }
    //}
}