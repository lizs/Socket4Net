using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProtoBuf;

namespace socket4net
{
    [ProtoContract]
    public class ProtoWrapper<T>
    {
        [ProtoMember(1)]
        public T Item { get; set; }
    }

    /// <summary>
    ///     对接Redis key-value
    /// </summary>
    public class RedisEntry
    {
        public string Feild { get; set; }
        public byte[] Data { get; set; }
    }

    /// <summary>
    ///     实体信息
    ///     一个实体由 类型、Id、属性定义
    /// </summary>
    public class EntityEntry
    {
        public Type Type { get; set; }
        public long Id { get; set; }
        public IReadOnlyCollection<RedisEntry> Blocks { get; set; }
    }


    public interface IRedisClient
    {
        bool HashMultiDel(string key, List<string> feilds);
        bool HashMultiSet(string key, List<RedisEntry> blocks);
        List<byte[]> HashGetAll(string key);
        List<byte[]> HashMultiGet(string key, List<string> feilds);
        long SortedSetLength(string key);
        bool SortedSetAdd(string key, byte[] value, double score);
        long SortedSetAdd(string key, List<Pair<byte[], double>> entries);
        List<byte[]> SortedSetRange(string key, long from, long to);
        List<Pair<byte[], double>> SortedSetRangeWithScores(string key, long from, long to);
        List<byte[]> SortedSetRangeByScore(string key, double from, double to);
        List<Pair<byte[], double>> SortedSetRangeByScoreWithScores(string key, long from, long to);
        string GetString(string key);
        byte[] Get(string key);
        bool Set(string key, string value);
        bool Set(string key, byte[] value);
        bool Exist(string key);
    }

    public interface IAsyncRedisClient : IRedisClient
    {
        Task<bool> HashMultiDelAsync(string key, List<string> feilds);
        Task<List<byte[]>> HashGetAllAsync(string key);
        Task<List<byte[]>> HashMultiGetAsync(string key, List<string> feilds);
        Task<long> SortedSetLengthAsync(string key);
        Task<bool> SortedSetRemoveAsync(string key, byte[] member);
        Task<long> SortedSetRemoveAsync(string key, List<byte[]> members);
        Task<long?> SortedSetRankAsync(string key, byte[] member);
        Task<bool> SortedSetAddAsync(string key, byte[] value, double score);
        Task<double> SortedSetIncAsync(string key, byte[] value, double score);
        Task<long> SortedSetAddAsync(string key, List<Pair<byte[], double>> entries);
        Task<List<byte[]>> SortedSetRangeAsync(string key, long from, long to);
        Task<List<Pair<byte[], double>>> SortedSetRangeWithScoresAsync(string key, long from, long to);
        Task<List<byte[]>> SortedSetRangeByScoreAsync(string key, double from, double to);
        Task<List<Pair<byte[], double>>> SortedSetRangeByScoreWithScoresAsync(string key, long from, long to);
        Task<string> GetStringAsync(string key);
        Task<byte[]> GetAsync(string key);
        Task<bool> SetAsync(string key, string value);
        Task<bool> SetAsync(string key, byte[] value);
        Task<bool> ExistAsync(string key);
    }
}
