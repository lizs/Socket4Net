
using System.Collections;
using System.Diagnostics;
using NUnit.Framework;

namespace socket4net.tests
{
    internal class ScheduleCase : Case
    {
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