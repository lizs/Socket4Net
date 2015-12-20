using Xunit;

namespace socket4net.tests
{
    internal class ComponentedObjCase : Case
    {
        [Fact]
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