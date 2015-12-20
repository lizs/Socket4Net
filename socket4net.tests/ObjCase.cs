using NUnit.Framework;

namespace socket4net.tests
{
    [TestFixture]
    internal class ObjCase : Case
    {
        [Test]
        internal override void Do()
        {
            var obj = Obj.Create<MyObj>(ObjArg.Empty);
            Assert.True(obj.Initialized);

            obj.Reset();
            Assert.True(obj.Reseted);

            obj.Start();
            Assert.True(obj.Started);

            obj.Destroy();
            Assert.True(obj.Destroyed);
        }
    }
}