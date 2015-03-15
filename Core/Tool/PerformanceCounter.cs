using System;
using Core.Log;
using Core.Net.TCP;

namespace Core.Tool
{
//    public class PerformanceCounter<TSession> : w
//    {
//        private Timer.Timer _timer;

//        public IPeer Target { get; private set; }
//        public void Start(IPeer target, int delay = 1000, int period = 5 * 1000)
//        {
//            Target = target;
//            _timer = new Timer.Timer("PerformanceCounter for " + target.EndPoint, 1000, 5 * 1000);

//            _timer.Start();
//            _timer.Arrived += OutputPerformance;
//        }

//        public void Stop()
//        {
//            _timer.Arrived -= OutputPerformance;
//            _timer.Stop();
//        }

//        private void OutputPerformance(Timer.Timer timer)
//        {
//            Logger.Instance.InfoFormat("{0} : Send[{1}]/Receive[{2}] Logic[{3}]/Net[{4}]/Sessions[{5}]", Target.EndPoint,
//                Session.SendCnt, Session.ReceiveCnt, Target);

//#if DEBUG
//            Logger.Instance.WarnFormat("Send : {0}, Receive : {1}", Session.SendCnt, Session.ReceiveCnt);
//#endif

//            Logger.Instance.WarnFormat("STA Jobs : {0}, Net jobs : {1}, Sessions :  {2}", Launcher.StaService.Jobs, Launcher.NetService.Jobs, SessionMgr.Count);
//            GC.Collect();
//        }
//    }
}