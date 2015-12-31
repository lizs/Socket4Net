
namespace socket4net.tests
{
    internal enum EComponentId : short
    {
        ComponentA,
        ComponentB,
    }

    internal class ComponentId : Key<short>
    {
        public ComponentId(short value)
            : base(value)
        {
        }

        public static implicit operator ComponentId(EComponentId cid)
        {
            return new ComponentId((short)cid);
        }

        public override string ToString()
        {
            return ((EComponentId) Value).ToString();
        }
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