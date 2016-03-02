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
            Obj.New<Launcher>(new LauncherArg(new Log4Net("log4net.config", "socket4net.tests")));
            Launcher.Ins.Start();
        }

        [TearDown]
        public virtual void Destroy()
        {
            Launcher.Ins.Destroy();
        }
    }
}