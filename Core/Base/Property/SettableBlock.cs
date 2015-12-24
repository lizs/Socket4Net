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
        public object PreviousValue { get; private set; }

        /// <summary>
        /// 暴露settter
        /// </summary>
        /// <param name="value"></param>
        public void Set(object value)
        {
            InternalSet(value);
        }

        /// <summary>
        /// 暴露settter
        /// </summary>
        /// <param name="value"></param>
        public void Set(TItem value)
        {
            InternalSet(value);
        }

        /// <summary>
        ///     cast旧值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T PreviousAs<T>()
        {
            return (T) PreviousValue;
        }

        protected override void InternalSet(object value)
        {
            PreviousValue = Value;
            base.InternalSet(value);
        }
    }
}