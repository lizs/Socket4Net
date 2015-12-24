
namespace socket4net.tests
{
    internal enum EComponentId
    {
        ComponentA,
        ComponentB,
    }

    [ComponentId((short)EComponentId.ComponentA)]
    internal class ComponentA : Component
    {
    }

    [ComponentId((short)EComponentId.ComponentB)]
    [DependOn(typeof(ComponentA))]
    internal class ComponentB : Component
    {
    }
}