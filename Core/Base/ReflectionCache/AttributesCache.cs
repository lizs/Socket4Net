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
using System;
using System.Collections.Generic;
using System.Linq;

namespace socket4net
{
    public abstract class AttributesCache<TValue, TAttribute> where TAttribute : Attribute
    {
        private readonly Dictionary<Type, TValue> _cache = new Dictionary<Type, TValue>();

        public TValue Get(Type type)
        {
            if (_cache.ContainsKey(type))
                return _cache[type];

            var usage =
                (AttributeUsageAttribute)
                    typeof (TAttribute).GetCustomAttributes(typeof (AttributeUsageAttribute), false).FirstOrDefault();

            var allowMultiple = usage == null || usage.AllowMultiple;
            var inherited = usage == null || usage.Inherited;

            var attributes = type.GetCustomAttributes(typeof(TAttribute), inherited)
                .Cast<TAttribute>();

            var ret = allowMultiple ? Handle(attributes) : Handle(attributes.FirstOrDefault());
            _cache[type] = ret;

            return ret;
        }

        protected abstract TValue Handle(IEnumerable<TAttribute> attributes);
        protected abstract TValue Handle(TAttribute attribute);

        public void Clear()
        {
            _cache.Clear();
        }
    }
}