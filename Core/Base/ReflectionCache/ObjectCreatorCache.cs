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
using System.Linq.Expressions;

namespace socket4net
{
    public static class ObjFactory
    {
        private static readonly Dictionary<Type, Func<Obj>> CtorCache = new Dictionary<Type, Func<Obj>>();

        private static Func<Obj> GetCtor(Type type)
        {
            if (CtorCache.ContainsKey(type)) return CtorCache[type];

            Expression body = Expression.New(type);
            var ret = Expression.Lambda<Func<Obj>>(body).Compile();
            CtorCache[type] = ret;
            return ret;
        }

        public static Obj Create(Type type, ObjArg arg, bool start)
        {
            var ctor = GetCtor(type);
            var obj = ctor();
            obj.Init(arg);

            if (start)
                obj.Start();

            return obj;
        }

        public static T Create<T>(ObjArg arg, bool start) where T : Obj
        {
            var obj = Create(typeof (T), arg, start);
            return (T) obj;
        }

        public static void ClearCtorCache()
        {
            CtorCache.Clear();
        }
    }
}
