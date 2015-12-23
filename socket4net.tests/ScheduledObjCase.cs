using System.Collections;
using CustomLog;
using NUnit.Framework;
using NUnit.Framework.Compatibility;

namespace socket4net.tests
{
    public class MyScheduledObj : ScheduledObj
    {
        protected override void OnStart()
        {
            base.OnStart();
            StartCoroutine(MyCoroutine);
        }

        private IEnumerator MyCoroutine()
        {
            var watch = new Stopwatch();
            watch.Start();

            Logger.Instance.Info("Hello coroutine");
            yield return WaitFor(2 * 1000);
            Logger.Instance.InfoFormat("{0}ms passed", watch.ElapsedMilliseconds);

            yield return WaitFor(2 * 1000);
            Logger.Instance.InfoFormat("{0}ms passed", watch.ElapsedMilliseconds);

            yield return SubCoroutine();
            Logger.Instance.InfoFormat("{0}ms passed", watch.ElapsedMilliseconds);

            watch.Stop();
        }

        private IEnumerator SubCoroutine()
        {
            yield return WaitFor(1 * 1000);
            yield return WaitFor(1 * 1000);
            yield return WaitFor(1 * 1000);
        }
    }

    internal class ScheduledObjCase : Case
    {
        private AutoLogicService _service;
        private ScheduledObj _obj;

        public override void Init()
        {
            base.Init();

            _service = Obj.Create<AutoLogicService>(new ServiceArg(null, 100000, 10));
            _service.Start();

            GlobalVarPool.Instance.Set(GlobalVarPool.NameOfLogger, new Log4Net());
            GlobalVarPool.Instance.Set(GlobalVarPool.NameOfLogicService, _service);

            _obj = Obj.Create<MyScheduledObj>(new ScheduledObjArg(null));
            _obj.Start();
        }

        public override void Destroy()
        {
            base.Destroy();
            _obj.Destroy();
            _service.Destroy();
        }

        //[Test]
        //public void TestCoroutine()
        //{
        //    var obj = Obj.Create<ScheduledObj>(new ScheduledObjArg(null));
        //    obj.StartCoroutine(MyCoroutine);
        //}
    }
}