namespace socket4net
{
    public class IncreasableBlock<TItem> : Block<TItem>, IIncreasableBlock<TItem>
    {
        public IncreasableBlock(short id, TItem value, EBlockMode mode,
            TItem lowerBound, TItem upperBound)
            : base(id, value, mode)
        {
            UpperBound = upperBound;
            LowerBound = lowerBound;

            PreviousValue = value;
        }

        /// <summary>
        ///     旧值
        /// </summary>
        public TItem PreviousValue { get; private set; }

        public TItem UpperBound { get; private set; }
        public TItem LowerBound { get; private set; }

        public override EBlockType EBlockType { get { return EBlockType.Increasable; } }
        
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

        protected override void InternalSet(TItem value)
        {
            PreviousValue = (TItem)Value;
            base.InternalSet(value);
        }
    }
}