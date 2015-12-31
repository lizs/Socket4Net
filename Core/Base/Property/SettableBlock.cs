namespace socket4net
{
    public class SettableBlock<TItem> : Block<TItem>, ISettableBlock<TItem>
    {
        public SettableBlock(short id, TItem value, EBlockMode mode)
            : base(id, value, mode)
        {
            PreviousValue = value;
        }

        public override EBlockType EBlockType { get { return EBlockType.Settable; } }

        /// <summary>
        ///     旧值
        /// </summary>
        public TItem PreviousValue { get; private set; }

        /// <summary>
        /// 暴露settter
        /// </summary>
        /// <param name="value"></param>
        public void Set(object value)
        {
            InternalSet((TItem)value);
        }

        /// <summary>
        /// 暴露settter
        /// </summary>
        /// <param name="value"></param>
        public void Set(TItem value)
        {
            InternalSet(value);
        }

        protected override void InternalSet(TItem value)
        {
            PreviousValue = (TItem)Value;
            base.InternalSet(value);
        }
    }
}