using System.Collections;
using CustomLog;
using NUnit.Framework;
using NUnit.Framework.Compatibility;

namespace socket4net.tests
{
    internal class CoroutineCase : Case
    {
        private AutoLogicService _service;

        public override void Init()
        {
            base.Init();

            _service = Obj.Create<AutoLogicService>(new ServiceArg(null, 100000, 10));
            _service.Start();

            GlobalVarPool.Instance.Set(GlobalVarPool.NameOfLogger, new Log4Net());
            GlobalVarPool.Instance.Set(GlobalVarPool.NameOfLogicService, _service);
        }

        public override void Destroy()
        {
            base.Destroy();
            _service.Destroy();
        }

        [Test]
        public void TestCoroutine()
        {
            var obj = Obj.Create<ScheduledObj>(new ScheduledObjArg(null));
            obj.StartCoroutine(MyCoroutine);
        }

        private IEnumerator MyCoroutine()
        {
            long elapsed = 0;
            var watch = new Stopwatch();
            watch.Start();

            Logger.Instance.Info("Hello coroutine");
            yield return ScheduledObj.WaitFor(2 * 1000);
            elapsed = watch.ElapsedMilliseconds;

            Logger.Instance.InfoFormat("{0}ms passed", elapsed);
            Assert.True(elapsed >= 2 * 1000);

            yield return ScheduledObj.WaitFor(2 * 1000);
            elapsed = watch.ElapsedMilliseconds;

            Logger.Instance.InfoFormat("{0}ms passed", elapsed);
            Assert.True(elapsed >= 4 * 1000);

            yield return SubCoroutine();
            elapsed = watch.ElapsedMilliseconds;
            Assert.True(elapsed >= 7 * 1000);
            Logger.Instance.InfoFormat("{0}ms passed", elapsed);

            watch.Stop();
        }

        private IEnumerator SubCoroutine()
        {
            yield return ScheduledObj.WaitFor(1 * 1000);
            yield return ScheduledObj.WaitFor(1 * 1000);
            yield return ScheduledObj.WaitFor(1 * 1000);
        }
    }
}