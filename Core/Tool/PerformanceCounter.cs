using System;
using Core.Log;
using Core.Net.TCP;
using Core.Service;

namespace Core.Tool
{
    public static class PerformanceCounter
    {
        private static readonly Timer.Timer _timer = new Timer.Timer("PerformanceCounter", 1000, 5 * 1000);

        public static void Run()
        {
            Launcher.PerformInSta(() =>
            {
                _timer.Start();
                _timer.Arrived += OutputPerformance;
            });
        }

        private static void OutputPerformance(Timer.Timer timer)
        {
#if DEBUG
            Log.WarnFormat("Send : {0}, Receive : {1}", Session.SendCnt, Session.ReceiveCnt);
#endif

            Logger.Instance.WarnFormat("STA Jobs : {0}, Net jobs : {1}, Sessions :  {2}", Launcher.StaService.Jobs, Launcher.NetService.Jobs, SessionMgr.Count);
            GC.Collect();
        }
    }
}