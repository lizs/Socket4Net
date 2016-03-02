using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ecs;
using socket4net;

namespace node
{
    public class AsyncRedisClient : RedisClient, IAsyncRedisClient
    {
        #region hash set
        public async Task<bool> HashMultiDelAsync(string hashid, List<string> feilds)
        {
            return await TaskHelper.ExcuteAsync(hashid, feilds, HashMultiDel);
        }

        public async Task<byte[][]> HashGetAllAsync(string hashid)
        {
            return await TaskHelper.ExcuteAsync(hashid, HashGetAll);
        }

        public async Task<byte[][]> HashMultiGetAsync(string hashid, List<string> feilds)
        {
            return await TaskHelper.ExcuteAsync(hashid, feilds, HashMultiGet);
        }
        #endregion

        #region sorted set
        public async Task<long> SortedSetLengthAsync(string hashid)
        {
            return await TaskHelper.ExcuteAsync(hashid, SortedSetLength);
        }

        public async Task<long> SortedSetRemoveAsync(string hashid, byte[] member)
        {
            return await TaskHelper.ExcuteAsync(hashid, k => SortedSetRemove(k, member));
        }

        public Task<long> SortedSetRemoveAsync(string key, List<byte[]> members)
        {
            return null;
        }

        public async Task<long> SortedSetRemoveAsync(string hashid, byte[][] values)
        {
            return await TaskHelper.ExcuteAsync(hashid, k => SortedSetRemove(k, values));
        }

        public async Task<long?> SortedSetRankAsync(string hashid, byte[] value)
        {
            return await TaskHelper.ExcuteAsync(hashid, k => SortedSetRank(k, value));
        }

        public async Task<long> SortedSetAddAsync(string hashid, byte[] value, double score)
        {
            return await TaskHelper.ExcuteAsync(hashid, k => SortedSetAdd(k, value, score));
        }

        public async Task<double> SortedSetIncAsync(string hashid, byte[] value, double score)
        {
            return await TaskHelper.ExcuteAsync(hashid, k => SortedSetInc(k, value, score));
        }

        public async Task<long> SortedSetAddAsync(string hashid, List<Pair<byte[], double>> entries)
        {
            return await TaskHelper.ExcuteAsync(hashid, k => SortedSetAdd(k, entries));
        }

        public async Task<byte[][]> SortedSetRangeAsync(string hashid, int from, int to)
        {
            return await TaskHelper.ExcuteAsync(hashid, k => SortedSetRange(k, from, to));
        }

        public async Task<byte[][]> SortedSetRangeWithScoresAsync(string hashid, int from, int to)
        {
            return await TaskHelper.ExcuteAsync(hashid, k => SortedSetRangeWithScores(k, from, to));
        }

        public async Task<byte[][]> SortedSetRangeByScoreAsync(string hashid, double from, double to)
        {
            return await TaskHelper.ExcuteAsync(hashid, k => SortedSetRangeByScore(k, from, to));
        }

        public Task<byte[][]> SortedSetRangeByScoreWithScoresAsync(string key, double @from, double to)
        {
            return null;
        }

        public async Task<byte[][]> SortedSetRangeByScoreWithScoresAsync(string hashid, long from, long to)
        {
            return await TaskHelper.ExcuteAsync(hashid, k => SortedSetRangeByScoreWithScores(k, from, to));
        }
        #endregion

        public async Task<string> GetStringAsync(string hashid)
        {
            return await TaskHelper.ExcuteAsync(hashid, GetString);
        }

        public async Task<byte[]> GetAsync(string hashid)
        {
            return await TaskHelper.ExcuteAsync(hashid, Get);
        }

        public async Task<bool> SetAsync(string hashid, string value)
        {
            return await TaskHelper.ExcuteAsync(hashid, value, Set);
        }

        public async Task<bool> SetAsync(string hashid, byte[] value)
        {
            return await TaskHelper.ExcuteAsync(hashid, value, Set);
        }

        public async Task<bool> ExistAsync(string hashid)
        {
            return await TaskHelper.ExcuteAsync(hashid, Exist);
        }
    }
}