#region MIT
//  /*The MIT License (MIT)
// 
//  Copyright 2016 lizs lizs4ever@163.com
//  
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//  
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
//  
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
//   * */
#endregion
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

    //        Logger.Ins.Info(sb.ToString());
    //        GC.Collect();
    //    }
    //}
}