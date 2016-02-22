using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ecs;
using socket4net;
using StackExchange.Redis;

namespace Sample
{
    public class RedisClientArg : UniqueObjArg<short>
    {
        public RedisClientArg(IObj parent, short key, string ip, string port, string pwd)
            : base(parent, key)
        {
            Ip = ip;
            Port = port;
            Pwd = pwd;
        }

        public string Ip { get; private set; }
        public string Port { get; private set; }
        public string Pwd { get; private set; }
    }

    public class RedisClient : UniqueObj<short>, IRedisClient
    {
        public string Ip { get; private set; }
        public string Port { get; private set; }
        public string Pwd { get; private set; }

        private readonly object _connectionLocker = new object();
        private ConnectionMultiplexer _connection;
        protected ConnectionMultiplexer Connection
        {
            get
            {
                if (_connection != null && _connection.IsConnected) return _connection;

                if(_connection != null)
                    _connection.Dispose();

                var ops = ConfigurationOptions.Parse(Ip + ":" + Port);
                ops.Password = Pwd;
                ops.AbortOnConnectFail = false;

                try
                {
                    _connection = ConnectionMultiplexer.Connect(ops);
                }
                catch (Exception e)
                {
                    Logger.Ins.Error("Redis连接失败。{0}:{1}", e.Message, e.StackTrace);
                    _connection = null;
                }

                return _connection;
            }
        }

        protected ConnectionMultiplexer Multiplexer
        {
            get
            {
                lock (_connectionLocker)
                {
                    return Connection;
                }
            }
        }

        private IServer _server;
        public IServer Server
        {
            get { return _server ?? (_server = Multiplexer.GetServer(Ip, Port)); }
        }

        protected override void OnInit(ObjArg arg)
        {
            base.OnInit(arg);
            var more = arg.As<RedisClientArg>();
            Ip = more.Ip;
            Port = more.Port;
            Pwd = more.Pwd;
        }

        protected override void OnStart()
        {
            base.OnStart();
            if (!Multiplexer.IsConnected)
            {
                Logger.Ins.Error("redis连接未建立（唤醒redis）");
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (Multiplexer != null)
                Multiplexer.Dispose();
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

        #region server interfaces

        //void Keys(string pattern)
        //{
        //    MyServer.Keys()
        //}

        #endregion

        #region hash set
        public bool HashMultiDel(string key, List<string> feilds)
        {
            if (feilds == null || feilds.Count == 0) return true;

            try
            {
                var db = Multiplexer.GetDatabase();
                var delCnt =
                    db.HashDeleteAsync(key, feilds.Select(x => (RedisValue) PiSerializer.SerializeValue(x)).ToArray())
                        .Result;

                if (delCnt != feilds.Count)
                    ;   // 缓存机制，所以请求删除的字段可能尚未在redis创建

                return true;
            }
            catch (Exception e)
            {
                LogException("HashMultiDel", e);
                return false;
            }
        }

        public bool HashMultiSet(string key, List<RedisEntry> blocks)
        {
            if (blocks == null || blocks.Count == 0) return true;

            var hashEntries = new List<HashEntry>();
            blocks.ForEach(block =>
            {
                if (!block.Data.IsNullOrEmpty())
                {
                    hashEntries.Add(new HashEntry(block.Feild, block.Data));
                }
            });

            if (hashEntries.Count == 0) return true;

            var ret = true;
            var db = Multiplexer.GetDatabase();
            try
            {
                Task.WaitAll(db.HashSetAsync(key, hashEntries.ToArray()));
            }
            catch (Exception e)
            {
                ret = false;
                LogException("HashMultiSet", e);
            }
            return ret;
        }

        public List<byte[]> HashGetAll(string key)
        {
            var data = new List<byte[]>();
            try
            {
                var db = Multiplexer.GetDatabase();
                var hashEntries = db.HashGetAllAsync(key);
                foreach (var entry in hashEntries.Result)
                {
                    data.Add(entry.Name);
                    data.Add(entry.Value);
                }
            }
            catch (Exception e)
            {
                data = null;
                LogException("HashGetAll", e);
            }

            return data;
        }

        public List<byte[]> HashMultiGet(string key, List<string> feilds)
        {
            if (feilds == null || feilds.Count == 0) return null;

            try
            {
                var db = Multiplexer.GetDatabase();
                var values = db.HashGetAsync(key, feilds.Select(x => (RedisValue)PiSerializer.SerializeValue(x)).ToArray());
                return values.Result.Select(value => (byte[]) value).ToList();
            }
            catch (Exception e)
            {
                LogException("HashMultiGet", e);
                return null;
            }
        }

        #endregion

        #region sorted set

        public long SortedSetLength(string key)
        {
            var db = Multiplexer.GetDatabase();
            return db.SortedSetLength(key);
        }

        public bool SortedSetAdd(string key, byte[] value, double score)
        {
            var db = Multiplexer.GetDatabase();
            return db.SortedSetAdd(key, value, score);
        }

        public long SortedSetAdd(string key, List<Pair<byte[], double>> entries)
        {
            var db = Multiplexer.GetDatabase();
            return db.SortedSetAdd(key, entries.Select(x=>new SortedSetEntry(x.Key, x.Value)).ToArray());
        }

        public List<byte[]> SortedSetRange(string key, long from, long to)
        {
            var db = Multiplexer.GetDatabase();
            var ret = db.SortedSetRangeByRank(key, from, to);
            if (ret.IsNullOrEmpty()) return null;
            return ret.Select(x=>(byte[])x).ToList();
        }

        public List<Pair<byte[], double>> SortedSetRangeWithScores(string key, long from, long to)
        {
            var db = Multiplexer.GetDatabase();
            var ret = db.SortedSetRangeByRankWithScores(key, from, to);
            if (ret.IsNullOrEmpty()) return null;
            return ret.Select(x => new Pair<byte[], double>(x.Element, x.Score)).ToList();
        }

        public List<byte[]> SortedSetRangeByScore(string key, double from, double to)
        {
            var db = Multiplexer.GetDatabase();
            var ret = db.SortedSetRangeByScore(key, from, to);
            if (ret.IsNullOrEmpty()) return null;
            return ret.Select(x => (byte[])x).ToList();
        }

        public List<Pair<byte[], double>> SortedSetRangeByScoreWithScores(string key, long from, long to)
        {
            var db = Multiplexer.GetDatabase();
            var ret = db.SortedSetRangeByScoreWithScores(key, from, to);
            if (ret.IsNullOrEmpty()) return null;
            return ret.Select(x => new Pair<byte[], double>(x.Element, x.Score)).ToList();
        }

        #endregion

        public string GetString(string key)
        {
            try
            {
                var db = Multiplexer.GetDatabase();
                return db.StringGet(key);
            }
            catch (Exception e)
            {
                LogException("Get", e);
                return string.Empty;
            }
        }

        public byte[] Get(string key)
        {
            try
            {
                var db = Multiplexer.GetDatabase();
                return db.StringGet(key);
            }
            catch (Exception e)
            {
                LogException("Get", e);
                return null;
            }
        }

        public bool Set(string key, string value)
        {
            try
            {
                var db = Multiplexer.GetDatabase();
                return db.StringSet(key, value);
            }
            catch (Exception e)
            {
                LogException("Set", e);
                return false;
            }
        }

        public bool Set(string key, byte[] value)
        {
            try
            {
                var db = Multiplexer.GetDatabase();
                return db.StringSet(key, value);
            }
            catch (Exception e)
            {
                LogException("Set", e);
                return false;
            }
        }

        public bool Exist(string key)
        {
            try
            {
                var db = Multiplexer.GetDatabase();
                return db.KeyExists(key);
            }
            catch (Exception e)
            {
                LogException("Exist", e);
                return false;
            }
        }
    }
}
