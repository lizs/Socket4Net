using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ecs;
using ServiceStack.Redis;
using socket4net;

namespace node
{
    public class RedisClientArg : UniqueObjArg<string>
    {
        public NodeElement Config { get; private set; }
        public RedisClientArg(IObj parent, string hashid, NodeElement cfg)
            : base(parent, hashid)
        {
            Config = cfg;
        }
    }

    public class RedisClient : UniqueObj<string>, ecs.IRedisClient
    {
        private PooledRedisClientManager _mgr;
        public NodeElement Config { get; private set; }

        protected override void OnInit(ObjArg arg)
        {
            base.OnInit(arg);
            Config = arg.As<RedisClientArg>().Config;

            _mgr = new PooledRedisClientManager(new[] {Config.Ip + ":" + Config.Port}, null,
                new RedisClientManagerConfig
                {
                    AutoStart = false,
                    MaxReadPoolSize = 10,
                    MaxWritePoolSize = 10,
                });
        }

        protected override void OnStart()
        {
            base.OnStart();
            _mgr.Start();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _mgr.Dispose();
        }

        private void LogException(string tag, Exception e)
        {
            if (e.InnerException != null)
            {
                Logger.Ins.Error("{0}:{1}:{2}", tag, e.InnerException.Message, e.InnerException.StackTrace);
            }
            else
            {
                Logger.Ins.Error("{0}:{1}:{2}", tag, e.Message, e.StackTrace);
            }
        }
        
        #region hash set
        public bool HashMultiDel(string hashid, List<string> feilds)
        {
            if (feilds == null || feilds.Count == 0) return true;

            try
            {
                using (var client = _mgr.GetClient() as RedisNativeClient)
                {
                    var delCnt = client.HDel(hashid, feilds.Select(PiSerializer.SerializeValue).ToArray());
                    if (delCnt != feilds.Count)
                        ;   // 缓存机制，所以请求删除的字段可能尚未在redis创建

                    return true;
                }
            }
            catch (Exception e)
            {
                LogException("HashMultiDel", e);
                return false;
            }
        }

        public bool HashMultiSet(string hashid, List<RedisEntry> blocks)
        {
            if (blocks.IsNullOrEmpty()) return true;

            var keys = new List<byte[]>();
            var values = new List<byte[]>();

            blocks.ForEach(block =>
            {
                if (block.Data.IsNullOrEmpty()) return;

                keys.Add(Encoding.Default.GetBytes(block.Feild));
                values.Add(block.Data);
            });

            if (keys.IsNullOrEmpty() || values.IsNullOrEmpty() || keys.Count != values.Count) return true;

            try
            {
                using (var client = _mgr.GetClient() as RedisNativeClient)
                {
                    client.HMSet(hashid, keys.ToArray(), values.ToArray());
                    return true;
                }
            }
            catch (Exception e)
            {
                LogException("HashMultiSet", e);
                return false;
            }
        }

        public byte[][] HashGetAll(string hashid)
        {
            try
            {
                using (var client = _mgr.GetClient() as RedisNativeClient)
                {
                    return client.HGetAll(hashid);
                }
            }
            catch (Exception e)
            {
                LogException("HashGetAll", e);
                return null;
            }
        }

        public byte[][] HashMultiGet(string hashid, List<string> feilds)
        {
            if (feilds.IsNullOrEmpty()) return null;
            try
            {
                using (var client = _mgr.GetClient() as RedisNativeClient)
                {
                    return client.HMGet(hashid, feilds.Select(x => Encoding.Default.GetBytes(x)).ToArray());
                }
            }
            catch (Exception e)
            {
                LogException("HashMultiGet", e);
                return null;
            }
        }
        #endregion

        #region sorted set

        public long SortedSetLength(string hashid)
        {
            using (var client = _mgr.GetClient() as RedisNativeClient)
            {
                return client.ZCard(hashid);
            }
        }

        public double SortedSetInc(string hashid, byte[] value, double score)
        {
            using (var client = _mgr.GetClient() as RedisNativeClient)
            {
                return client.ZIncrBy(hashid, score, value);
            }
        }

        public long SortedSetRemove(string hashid, byte[][] values)
        {
            using (var client = _mgr.GetClient() as RedisNativeClient)
            {
                return client.ZRem(hashid, values);
            }
        }

        public long SortedSetRemove(string hashid, byte[] value)
        {
            using (var client = _mgr.GetClient() as RedisNativeClient)
            {
                return client.ZRem(hashid, value);
            }
        }

        public long SortedSetAdd(string hashid, byte[] value, double score)
        {
            using (var client = _mgr.GetClient() as RedisNativeClient)
            {
                return client.ZAdd(hashid, score, value);
            }
        }

        public long SortedSetAdd(string hashid, List<Pair<byte[], double>> entries)
        {
            using (var client = _mgr.GetClient() as RedisNativeClient)
            {
                return client.ZAdd(hashid, entries.Select(x => new KeyValuePair<byte[], double>(x.Key, x.Value)).ToList());
            }
        }

        public byte[][] SortedSetRange(string hashid, int from, int to)
        {
            using (var client = _mgr.GetClient() as RedisNativeClient)
            {
                return client.ZRange(hashid, from, to);
            }
        }

        public long SortedSetRank(string hashid, byte[] value)
        {
            using (var client = _mgr.GetClient() as RedisNativeClient)
            {
                return client.ZRank(hashid, value);
            }
        }
        
        public byte[][] SortedSetRangeWithScores(string hashid, int from, int to)
        {
            using (var client = _mgr.GetClient() as RedisNativeClient)
            {
                return client.ZRangeWithScores(hashid, from, to);
            }
        }

        public byte[][] SortedSetRangeByScore(string hashid, double from, double to)
        {
            using (var client = _mgr.GetClient() as RedisNativeClient)
            {
                return client.ZRangeByScore(hashid, from, to, null, null);
            }
        }

        public byte[][] SortedSetRangeByScoreWithScores(string hashid, double from, double to)
        {
            using (var client = _mgr.GetClient() as RedisNativeClient)
            {
                return client.ZRangeByScoreWithScores(hashid, from, to, null, null);
            }
        }

        #endregion

        public string GetString(string hashid)
        {
            try
            {
                using (var client = _mgr.GetClient() as RedisNativeClient)
                {
                    return Encoding.Default.GetString(client.Get(hashid));
                }
            }
            catch (Exception e)
            {
                LogException("Get", e);
                return string.Empty;
            }
        }

        public byte[] Get(string hashid)
        {
            try
            {
                using (var client = _mgr.GetClient() as RedisNativeClient)
                {
                    return client.Get(hashid);
                }
            }
            catch (Exception e)
            {
                LogException("Get", e);
                return null;
            }
        }

        public bool Set(string hashid, string value)
        {
            try
            {
                using (var client = _mgr.GetClient() as RedisNativeClient)
                {
                    client.Set(hashid, Encoding.Default.GetBytes(value));
                    return true;
                }
            }
            catch (Exception e)
            {
                LogException("Set", e);
                return false;
            }
        }

        public bool Set(string hashid, byte[] value)
        {
            try
            {
                using (var client = _mgr.GetClient() as RedisNativeClient)
                {
                    client.Set(hashid, value);
                    return true;
                }
            }
            catch (Exception e)
            {
                LogException("Set", e);
                return false;
            }
        }

        public bool Exist(string hashid)
        {
            try
            {
                using (var client = _mgr.GetClient() as RedisNativeClient)
                {
                    return client.Exists(hashid) > 0;
                }
            }
            catch (Exception e)
            {
                LogException("Exist", e);
                return false;
            }
        }
    }
}
