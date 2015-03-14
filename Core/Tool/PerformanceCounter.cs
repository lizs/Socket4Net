using System;
using Core.Net.TCP;
using Core.Service;

namespace Core.Tool
{
    public static class PerformanceCounter
    {
        private static readonly Timer.Timer _timer = new Timer.Timer("PerformanceCounter", 1000, 5 * 1000);
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(PerformanceCounter));

        public static void Run()
        {
            StaService.Perform(() =>
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

            Log.WarnFormat("STA Jobs : {0}, Net jobs : {1}, Sessions :  {2}", StaService.Instance.Count, NetService.Jobs, SessionMgr.Count);
            GC.Collect();
        }
    }
}