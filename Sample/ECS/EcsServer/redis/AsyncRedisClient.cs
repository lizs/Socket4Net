using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ecs;
using socket4net;
using StackExchange.Redis;

namespace Sample
{
    public class AsyncRedisClient : RedisClient, IAsyncRedisClient
    {
        #region hash set
        public async Task<bool> HashMultiDelAsync(string key, List<string> feilds)
        {
            return await TaskHelper.ExcuteAsync(key, feilds, HashMultiDel);
        }

        public async Task<List<byte[]>> HashGetAllAsync(string key)
        {
            return await TaskHelper.ExcuteAsync(key, HashGetAll);
        }

        public async Task<List<byte[]>> HashMultiGetAsync(string key, List<string> feilds)
        {
            return await TaskHelper.ExcuteAsync(key, feilds, HashMultiGet);
        }
        #endregion

        #region sorted set
        public async Task<long> SortedSetLengthAsync(string key)
        {
            Func<string, Task<long>> lambda = async k =>
            {
                var db = Multiplexer.GetDatabase();
                return await db.SortedSetLengthAsync(key);
            };

            return await await TaskHelper.ExcuteAsync(key, lambda);
        }

        public async Task<bool> SortedSetRemoveAsync(string key, byte[] member)
        {
            Func<string, Task<bool>> lambda = async k =>
            {
                var db = Multiplexer.GetDatabase();
                return await db.SortedSetRemoveAsync(key, member);
            };

            return await await TaskHelper.ExcuteAsync(key, lambda);
        }

        public async Task<long> SortedSetRemoveAsync(string key, List<byte[]> members)
        {
            Func<string, Task<long>> lambda = async k =>
            {
                var db = Multiplexer.GetDatabase();
                return await db.SortedSetRemoveAsync(key, members.Select(x => (RedisValue)x).ToArray());
            };

            return await await TaskHelper.ExcuteAsync( key, lambda);
        }

        public async Task<long?> SortedSetRankAsync(string key, byte[] member)
        {
            Func<string, Task<long?>> lambda = async k =>
            {
                var db = Multiplexer.GetDatabase();
                return await db.SortedSetRankAsync(key, member, Order.Descending);
            };

            return await await TaskHelper.ExcuteAsync( key, lambda);
        }

        public async Task<bool> SortedSetAddAsync(string key, byte[] value, double score)
        {
            Func<string, byte[], double, Task<bool>> lambda = async (k, v, s) =>
            {
                var db = Multiplexer.GetDatabase();
                return await db.SortedSetAddAsync(k, v, s);
            };

            return await await TaskHelper.ExcuteAsync( key, value, score, lambda);
        }

        public async Task<double> SortedSetIncAsync(string key, byte[] value, double score)
        {
            Func<string, byte[], double, Task<Double>> lambda = async (k, v, s) =>
            {
                var db = Multiplexer.GetDatabase();
                return await db.SortedSetIncrementAsync(k, v, s);
            };

            return await await TaskHelper.ExcuteAsync( key, value, score, lambda);
        }

        public async Task<long> SortedSetAddAsync(string key, List<Pair<byte[], double>> entries)
        {
            Func<string, List<Pair<byte[], double>>, Task<long>> lambda = async (k, v) =>
            {
                var db = Multiplexer.GetDatabase();
                return await db.SortedSetAddAsync(k, v.Select(x => new SortedSetEntry(x.Key, x.Value)).ToArray());
            };

            return await await TaskHelper.ExcuteAsync( key, entries, lambda);
        }

        public async Task<List<byte[]>> SortedSetRangeAsync(string key, long from, long to)
        {
            Func<string, long, long, Task<List<byte[]>>> lambda = async (k, f, t) =>
            {
                var db = Multiplexer.GetDatabase();
                var ret = await db.SortedSetRangeByRankAsync(k, f, t, Order.Descending);
                if (ret.IsNullOrEmpty()) return null;
                return ret.Select(x => (byte[])x).ToList();
            };

            return await await TaskHelper.ExcuteAsync( key, from, to, lambda);
        }

        public async Task<List<Pair<byte[], double>>> SortedSetRangeWithScoresAsync(string key, long from, long to)
        {
            Func<string, long, long, Task<List<Pair<byte[], double>>>> lambda = async (k, f, t) =>
            {
                var db = Multiplexer.GetDatabase();
                var ret = await db.SortedSetRangeByRankWithScoresAsync(k, f, t, Order.Descending);
                if (ret.IsNullOrEmpty()) return null;
                return ret.Select(x => new Pair<byte[], double>(x.Element, x.Score)).ToList();
            };

            return await await TaskHelper.ExcuteAsync( key, from, to, lambda);
        }

        public async Task<List<byte[]>> SortedSetRangeByScoreAsync(string key, double from, double to)
        {
            Func<string, double, double, Task<List<byte[]>>> lambda = async (k, f, t) =>
            {
                var db = Multiplexer.GetDatabase();
                var ret = await db.SortedSetRangeByScoreAsync(k, f, t, Exclude.None, Order.Descending);
                if (ret.IsNullOrEmpty()) return null;
                return ret.Select(x => (byte[])x).ToList();
            };

            return await await TaskHelper.ExcuteAsync( key, from, to, lambda);
        }

        public Task<List<Pair<byte[], double>>> SortedSetRangeByScoreWithScoresAsync(string key, long from, long to)
        {
            var db = Multiplexer.GetDatabase();
            var ret = db.SortedSetRangeByScoreWithScores(key, from, to, Exclude.None, Order.Descending);
            return ret.IsNullOrEmpty()
                ? Task.FromResult(default(List<Pair<byte[], double>>))
                : Task.FromResult(ret.Select(x => new Pair<byte[], double>(x.Element, x.Score)).ToList());
        }
        #endregion

        public async Task<string> GetStringAsync(string key)
        {
            return await TaskHelper.ExcuteAsync(key, GetString);
        }

        public async Task<byte[]> GetAsync(string key)
        {
            return await TaskHelper.ExcuteAsync(key, Get);
        }

        public async Task<bool> SetAsync(string key, string value)
        {
            return await TaskHelper.ExcuteAsync(key, value, Set);
        }

        public async Task<bool> SetAsync(string key, byte[] value)
        {
            return await TaskHelper.ExcuteAsync(key, value, Set);
        }

        public async Task<bool> ExistAsync(string key)
        {
            return await TaskHelper.ExcuteAsync(key, Exist);
        }
    }
}