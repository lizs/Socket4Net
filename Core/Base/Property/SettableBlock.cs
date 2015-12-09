namespace socket4net
{
    public class SettableBlock<TKey, TItem> : Block<TKey, TItem>, ISettableBlock<TKey, TItem>
    {
        public SettableBlock(PropertyBody<TKey> host, TKey id, TItem value, EBlockMode mode)
            : base(host, id, value, mode)
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