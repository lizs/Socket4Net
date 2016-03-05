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
using System.Threading.Tasks;
using socket4net;

namespace ecs
{
    /// <summary>
    ///     存储系统
    /// </summary>
    public class PersistSys : DataSys
    {
        private bool _busy;

        /// <summary>
        ///     存储
        /// </summary>
        public async Task<bool> PersistAsync(IAsyncRedisClient client)
        {
            if (client == null || _busy) return false;
            _busy = true;

            // 拷贝缓存（防止容器在异步返回之前被修改）
            var tmpDestroyCache = DestroyCache.ToDictionary(kv => kv.Key, kv => kv.Value);
            var tmpUpdateCache = UpdateCache.ToDictionary(kv => kv.Key,
                kv => new Pair<Type, List<IBlock>>(kv.Value.Key, kv.Value.Value.ToList()));

            // 删除
            var destroyRet = true;
            if (!tmpDestroyCache.IsNullOrEmpty())
            {
                destroyRet =
                    await
                        client.HashMultiDelAsync(Es.Key,
                            tmpDestroyCache.Select(x => Es.FormatFeild(x.Key, x.Value)).ToList());
                if (destroyRet)
                {
                    foreach (var item in tmpDestroyCache)
                    {
                        DestroyCache.Remove(item.Key);
                    }
                }
            }

            // 存储
            var entries =
                (from kv in tmpUpdateCache
                    where !kv.Value.Value.IsNullOrEmpty()
                    from item in kv.Value.Value
                    select Es.FormatBlock(kv.Key, item)).ToList();

            var setRet = client.HashMultiSet(Es.Key, entries);
            if (!setRet) return false;

            foreach (var item in tmpUpdateCache)
            {
                UpdateCache.Remove(item.Key);
            }

            _busy = false;
            return destroyRet;
        }
    }
}
