using NUnit.Framework;

namespace socket4net.tests
{
    [TestFixture]
    internal class ComponentedObjCase : Case
    {
        [Test]
        internal override void Do()
        {
            var obj = Obj.Create<MyComponentedObj>(new ComponentedObjArg<EProperty>(null, 1, null));

            var cp = obj.GetComponent<MyComponent>();
            Assert.NotNull(cp);
            Assert.True(cp.Initialized);

            obj.Start();
            Assert.True(cp.Started);

            obj.Destroy();
            Assert.True(cp.Destroyed);
        }
    }
}