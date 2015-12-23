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
            GlobalVarPool.Instance.Set(GlobalVarPool.NameOfLogger, new Log4Net("log4net.config", "socket4net.tests"));
        }
        [TearDown]
        public virtual void Destroy() { }
    }
}