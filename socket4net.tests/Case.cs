using CustomLog;
using NUnit.Framework;

namespace socket4net.tests
{
    [TestFixture]
    internal abstract class Case
    {
        [SetUp]
        public virtual void Init()
        {
            Obj.Create<Launcher>(new LauncherArg(null, new CustomLog.Log4Net("log4net.config", "socket4net.tests")));
            Launcher.Instance.Start();
        }

        [TearDown]
        public virtual void Destroy()
        {
            Launcher.Instance.Destroy();
        }
    }
}