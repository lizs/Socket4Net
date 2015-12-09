namespace socket4net
{
    public class IncreasableBlock<TKey, TItem> : Block<TKey, TItem>, IIncreasableBlock<TKey, TItem>
    {
        public IncreasableBlock(PropertyBody<TKey> host, TKey id, TItem value, EBlockMode mode,
            TItem upperBound, TItem lowerBound)
            : base(host, id, value, mode)
        {
            UpperBound = upperBound;
            LowerBound = lowerBound;

            PreviousValue = value;
        }

        /// <summary>
        ///     旧值
        /// </summary>
        public object PreviousValue { get; private set; }

        public TItem UpperBound { get; private set; }
        public TItem LowerBound { get; private set; }

        public override EBlockType EBlockType { get { return EBlockType.Increasable; } }

        public void Inc(object delta, out object overflow)
        {
            overflow = default(TItem);

            TItem of;
            Inc((TItem) delta, out of);
            overflow = of;
        }

        public void IncTo(object target)
        {
            IncTo((TItem)target);
        }

        public void Inc(TItem delta, out TItem overflow)
        {
            overflow = default(TItem);
            var newValue = (TItem)Value;
            newValue = Add<TItem>.Function(delta, newValue);

            if (GreaterThan<TItem>.Function(newValue, UpperBound))
            {
                InternalSet(UpperBound);
                overflow = Subtract<TItem>.Function(newValue, UpperBound);
            }
            else if (LessThan<TItem>.Function(newValue, LowerBound))
            {
                InternalSet(LowerBound);
                overflow = Subtract<TItem>.Function(newValue, LowerBound);
            }
            else
                InternalSet(newValue);
        }

        public void IncTo(TItem target)
        {
            if (GreaterThan<TItem>.Function(target, UpperBound))
                InternalSet(UpperBound);
            else if (LessThan<TItem>.Function(target, LowerBound))
                InternalSet(LowerBound);
            else
                InternalSet(target);
        }

        /// <summary>
        ///     cast旧值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T PreviousAs<T>()
        {
            return (T)PreviousValue;
        }

        protected override void InternalSet(object value)
        {
            PreviousValue = Value;
            base.InternalSet(value);
        }
    }
}