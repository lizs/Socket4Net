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
    public abstract class UniqueObjArg<TKey> : ObjArg
    {
        public TKey Key { get; private set; }

        protected UniqueObjArg(IObj owner, TKey key)
            : base(owner)
        {
            Key = key;
        }
    }

    public interface IUniqueObj<out TKey> : IObj
    {
        TKey Id { get; }
    }

    /// <summary>
    ///     拥有唯一Id（容器内唯一，不一定是Guid）
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public abstract class UniqueObj<TKey> : Obj, IUniqueObj<TKey>
    {
        protected override void OnInit(ObjArg arg)
        {
            base.OnInit(arg);

            var more = arg.As<UniqueObjArg<TKey>>();
            Id = more.Key;
        }

        /// <summary>
        ///     唯一Id
        /// </summary>
        public TKey Id { get; protected set; }

        /// <summary>
        ///     名字
        /// </summary>
        public override string Name
        {
            get { return string.Format("{0}:{1}", base.Name, Id); }
        }
    }
}