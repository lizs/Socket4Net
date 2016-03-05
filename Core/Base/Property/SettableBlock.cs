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