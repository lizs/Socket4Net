using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProtoBuf;
using socket4net;

namespace ecs
{
    [ProtoContract]
    public class ProtoWrapper<T>
    {
        [ProtoMember(1)]
        public T Item { get; set; }
    }

    /// <summary>
    ///     对接Redis hashid-value
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
        bool HashMultiSet(string hashid, List<RedisEntry> blocks);
        byte[][] HashGetAll(string key);
        byte[][] HashMultiGet(string hashid, List<string> feilds);
        long SortedSetLength(string key);
        long SortedSetAdd(string key, byte[] value, double score);
        long SortedSetAdd(string key, List<Pair<byte[], double>> entries);
        byte[][] SortedSetRange(string key, int from, int to);
        byte[][] SortedSetRangeWithScores(string key, int from, int to);
        byte[][] SortedSetRangeByScore(string key, double from, double to);
        byte[][] SortedSetRangeByScoreWithScores(string key, double from, double to);
        string GetString(string key);
        byte[] Get(string key);
        bool Set(string key, string value);
        bool Set(string key, byte[] value);
        bool Exist(string key);
    }

    public interface IAsyncRedisClient : IRedisClient
    {
        Task<bool> HashMultiDelAsync(string key, List<string> feilds);
        Task<byte[][]> HashGetAllAsync(string key);
        Task<byte[][]> HashMultiGetAsync(string key, List<string> feilds);
        Task<long> SortedSetLengthAsync(string key);
        Task<long> SortedSetRemoveAsync(string key, byte[] member);
        Task<long> SortedSetRemoveAsync(string key, List<byte[]> members);
        Task<long?> SortedSetRankAsync(string key, byte[] member);
        Task<long> SortedSetAddAsync(string key, byte[] value, double score);
        Task<double> SortedSetIncAsync(string key, byte[] value, double score);
        Task<long> SortedSetAddAsync(string key, List<Pair<byte[], double>> entries);
        Task<byte[][]> SortedSetRangeAsync(string key, int from, int to);
        Task<byte[][]> SortedSetRangeWithScoresAsync(string key, int from, int to);
        Task<byte[][]> SortedSetRangeByScoreAsync(string key, double from, double to);
        Task<byte[][]> SortedSetRangeByScoreWithScoresAsync(string key, double from, double to);
        Task<string> GetStringAsync(string key);
        Task<byte[]> GetAsync(string key);
        Task<bool> SetAsync(string key, string value);
        Task<bool> SetAsync(string key, byte[] value);
        Task<bool> ExistAsync(string key);
    }
}
