using System;
using ecs;
using NUnit.Framework;

namespace socket4net.tests
{
    enum EEntityGroup
    {
        One,
        Two,
    }

    internal class EntityCase : Case
    {
        [Test]
        public void TestComponent()
        {
            var obj = Obj.New<Entity>(new EntityArg(null, 1));

            obj.AddComponent<ComponentA>();
            obj.AddComponent<ComponentB>();

            var cp = obj.GetComponent<ComponentA>();
            Assert.NotNull(cp);
            Assert.True(cp.Initialized);

            obj.Start();
            Assert.True(cp.Started);

            obj.Destroy();
            Assert.True(cp.Destroyed);
        }

        [Test]
        public void TestDependOn()
        {
            var obj = Obj.New<Entity>(new EntityArg(null, 1));

            obj.AddComponent<ComponentB>();

            var cp = obj.GetComponent<ComponentA>();
            Assert.NotNull(cp);
            Assert.True(cp.Initialized);

            obj.Start();
            Assert.True(cp.Started);

            obj.Destroy();
            Assert.True(cp.Destroyed);
        }
    }
}