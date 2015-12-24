
using System.Collections;
using System.Diagnostics;
using NUnit.Framework;

namespace socket4net.tests
{
    internal class ScheduleCase : Case
    {
        private AutoLogicService _service;

        public override void Init()
        {
            base.Init();

            _service = Obj.Create<AutoLogicService>(new ServiceArg(null, 100000, 10));
            _service.Start();
            GlobalVarPool.Instance.Set(GlobalVarPool.NameOfLogicService, _service);

            Obj.Create<ScheduleSys>(new ObjArg(null));
            ScheduleSys.Instance.Start();
        }

        public override void Destroy()
        {
            base.Destroy();
            ScheduleSys.Instance.Destroy();
            _service.Destroy();
        }

        [Test]
        public void TestCoroutine()
        {
            ScheduleSys.Instance.StartCoroutine(MyCoroutine);
        }

        private IEnumerator MyCoroutine()
        {
            var watch = new Stopwatch();
            watch.Start();

            Logger.Instance.Info("Hello coroutine");
            yield return ScheduleSys.WaitFor(2 * 1000);
            Logger.Instance.InfoFormat("{0}ms passed", watch.ElapsedMilliseconds);

            yield return ScheduleSys.WaitFor(2 * 1000);
            Logger.Instance.InfoFormat("{0}ms passed", watch.ElapsedMilliseconds);

            yield return SubCoroutine();
            Logger.Instance.InfoFormat("{0}ms passed", watch.ElapsedMilliseconds);

            watch.Stop();
        }

        private IEnumerator SubCoroutine()
        {
            yield return ScheduleSys.WaitFor(1 * 1000);
            yield return ScheduleSys.WaitFor(1 * 1000);
            yield return ScheduleSys.WaitFor(1 * 1000);
        }
    }
}