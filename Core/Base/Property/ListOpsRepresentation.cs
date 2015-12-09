namespace socket4net
{
    public abstract class ListOpsRepresentation : IBlockOps
    {
        public abstract EPropertyOps Ops { get; }

        /// <summary>
        ///     ²Ù×÷Ä¿±êId
        /// </summary>
        public int Id { get; set; }

        protected ListOpsRepresentation(int id)
        {
            Id = id;
        }
    }

    public class ListInsertOps : ListOpsRepresentation
    {
        public ListInsertOps(int id, int idx)
            : base(id)
        {
            Index = idx;
        }

        public override EPropertyOps Ops
        {
            get { return EPropertyOps.Insert; }
        }

        public int Index { get; set; }
    }

    public class ListRemoveOps : ListOpsRepresentation
    {
        public ListRemoveOps(int id) : base(id)
        {
        }

        public override EPropertyOps Ops
        {
            get { return EPropertyOps.Remove; }
        }
    }

    public class ListUpdateOps : ListOpsRepresentation
    {
        public ListUpdateOps(int id) : base(id)
        {
        }

        public override EPropertyOps Ops
        {
            get { return EPropertyOps.Update; }
        }
    }
}