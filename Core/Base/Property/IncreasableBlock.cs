#region MIT
//  /*The MIT License (MIT)
// 
//  Copyright 2016 lizs lizs4ever@163.com
//  
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//  
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
//  
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
//   * */
#endregion
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