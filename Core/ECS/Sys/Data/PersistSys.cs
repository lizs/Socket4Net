using System.Linq;
using System.Threading.Tasks;

namespace socket4net
{
    /// <summary>
    ///     存储系统（针对Redis）
    /// </summary>
    public class PersistSys : DataSys
    {
        private bool _busy = false;

        /// <summary>
        ///     存储
        /// </summary>
        public async Task<bool> PersistAsync(IAsyncRedisClient client)
        {
            if(_busy) return false;
            _busy = true;

            // 拷贝缓存
            var tmpDestroyCache = DestroyCache.ToDictionary(kv => kv.Key, kv => kv.Value);
            var tmpUpdateCache = UpdateCache.ToDictionary(kv => kv.Key, kv => kv.Value.ToList());

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
                    where !kv.Value.IsNullOrEmpty()
                    from item in kv.Value
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
