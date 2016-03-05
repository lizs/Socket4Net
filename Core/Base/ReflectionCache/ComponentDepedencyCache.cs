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
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class DependOnAttribute : Attribute
    {
        public Type[] Targets { get; private set; }
        public DependOnAttribute(params Type[]targets)
        {
            Targets = targets.ToArray();
        }
    }

    public class ComponentDepedencyCache : AttributesCache<Type[], DependOnAttribute>
    {
        private static ComponentDepedencyCache _ins;
        public static ComponentDepedencyCache Ins
        {
            get { return _ins ?? (_ins = new ComponentDepedencyCache()); }
        }

        protected override Type[] Handle(IEnumerable<DependOnAttribute> attributes)
        {
            var dependOnAttributes = attributes as DependOnAttribute[] ?? attributes.ToArray();
            return dependOnAttributes.IsNullOrEmpty()
                ? new Type[] {}
                : dependOnAttributes.SelectMany(x => x.Targets).ToArray();
        }

        protected override Type[] Handle(DependOnAttribute attribute)
        {
            throw new NotImplementedException("");
        }
    }
}