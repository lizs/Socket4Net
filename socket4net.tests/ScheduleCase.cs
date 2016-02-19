
using System.Collections;
using System.Diagnostics;
using ecs;
using NUnit.Framework;

namespace socket4net.tests
{
    internal class ScheduleObj : Obj{}

    internal class ScheduleCase : Case
    {
        private Obj _scheduObj;
        [Test]
        public void TestCoroutine()
        {
            _scheduObj = Obj.Create<ScheduleObj>(ObjArg.Empty);
            _scheduObj.StartCoroutine(MyCoroutine);
        }

        private IEnumerator MyCoroutine()
        {
            var watch = new Stopwatch();
            watch.Start();

            Logger.Ins.Info("Hello coroutine");
            yield return _scheduObj.WaitFor(2 * 1000);
            Logger.Ins.Info("{0}ms passed", watch.ElapsedMilliseconds);

            yield return _scheduObj.WaitFor(2 * 1000);
            Logger.Ins.Info("{0}ms passed", watch.ElapsedMilliseconds);

            yield return SubCoroutine();
            Logger.Ins.Info("{0}ms passed", watch.ElapsedMilliseconds);

            watch.Stop();
        }

        private IEnumerator SubCoroutine()
        {
            yield return _scheduObj.WaitFor(1 * 1000);
            yield return _scheduObj.WaitFor(1 * 1000);
            yield return _scheduObj.WaitFor(1 * 1000);
        }
    }
}