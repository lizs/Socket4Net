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
using socket4net;

namespace ecs
{
    public interface IProperty
    {
        #region Êý¾Ý²Ù×÷

        void Inject(IEnumerable<IBlock> blocks);
        List<T> GetList<T>(short pid);
        bool Inject(IBlock block);
        bool Inc<T>(short pid, T delta);
        bool Inc(short pid, object delta);
        bool Inc<T>(short pid, T delta, out T overflow);
        bool Inc(short pid, object delta, out object overflow);
        bool IncTo<T>(short pid, T target);
        bool IncTo(short pid, object target);
        bool Set<T>(short pid, T value);
        bool Set(short pid, object value);
        int IndexOf<T>(short pid, T item);
        int IndexOf<T>(short pid, Predicate<T> condition);
        T GetByIndex<T>(short pid, int idx);
        bool Add<T>(short pid, T value);
        bool Add(short pid, object value);
        bool AddRange<T>(short pid, List<T> items);
        bool Remove<T>(short pid, T item);
        bool Remove(short pid, object item);
        bool RemoveAll<T>(short pid, Predicate<T> predicate);
        bool RemoveAll<T>(short pid, Predicate<T> predicate, out int count);
        bool RemoveAll(short pid);
        bool RemoveAll(short pid, out int count);
        bool Insert<T>(short pid, int idx, T item);
        bool Insert(short pid, int idx, object item);
        bool Replace<T>(short pid, int idx, T item);
        bool Swap<T>(short pid, int idxA, int idxB);

        #endregion
    }
}