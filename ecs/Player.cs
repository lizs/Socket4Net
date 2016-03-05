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
using socket4net;
#if NET45
using System.Threading.Tasks;
#endif

namespace ecs
{
    public class PlayerArg : UniqueObjArg<long>
    {
        public PlayerArg(IObj owner, long key, IRpcSession session)
            : base(owner, key)
        {
            Session = session;
        }

        public IRpcSession Session { get; private set; }
    }

    public class FlushablePlayerArg : PlayerArg
    {
        public FlushablePlayerArg(IObj owner, long key, IRpcSession session, bool persistEnabled,
            Func<IAsyncRedisClient> redisClientGetter) : base(owner, key, session)
        {
            PersistEnabled = persistEnabled;
            RedisClientGetter = redisClientGetter;
        }

        public bool PersistEnabled { get; private set; }
        public Func<IAsyncRedisClient> RedisClientGetter { get; private set; }
    }

    /// <summary>
    ///     玩家
    ///     挂载系统如下：
    ///     1、 实体系统
    ///     2、 同步系统
    ///     3、 存储系统
    /// </summary>
    public abstract class Player : Entity
    {
        public EntitySys Es { get; protected set; }
        public SyncSys Ss { get; private set; }

        public IRpcSession Session { get; private set; }
        protected override void OnInit(ObjArg arg)  
        {
            base.OnInit(arg);
            var more = arg.As<PlayerArg>();

            Session = more.Session;
            Es = CreateEntitySys();
            Ss = New<SyncSys>(new DataSysArg(this, Es));
        }

        protected override void OnStart()
        {
            base.OnStart();
            if (Ss != null)
                Ss.Start();
            Es.Start();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if(Ss != null)
                Ss.Destroy();

            Es.Destroy();
        }

        protected abstract EntitySys CreateEntitySys();

#if NET45
        public async Task<RpcResult> OnRequest(RpcRequest rq)
        {
            var entity = rq.ObjId != 0 ? Es.Get(rq.ObjId) : this;
            if (entity == null) return RpcResult.Failure;

            if (rq.ComponentId == 0)
                return await entity.OnRequest(rq.Ops, rq.Data);

            var cp = entity.GetComponent(rq.ComponentId);
            return cp == null
                ? RpcResult.Failure
                : await cp.OnRequest(rq.Ops, rq.Data);
        }

        public async Task<bool> OnPush(RpcPush rp)
        {
            var entity = rp.ObjId != 0 ? Es.Get(rp.ObjId) : this;
            if (entity == null) return false;

            if (rp.ComponentId == 0)
                return await entity.OnPush(rp.Ops, rp.Data);

            var cp = entity.GetComponent(rp.ComponentId);
            return cp != null && await cp.OnPush(rp.Ops, rp.Data);
        }
#else
        public void OnRequest(RpcRequest rq, Action<RpcResult> cb)
        {
            var entity = rp.ObjId != 0 ? Es.Get(rp.ObjId) : this;
            if (entity == null)
            {
                cb(RpcResult.Failure);
                return;
            }

            if (rq.ComponentId == 0)
            {
                entity.OnMessageAsync(new NetReqMsg(rp.Ops, rp.Data), cb);
                return;
            }

            var cp = entity.GetComponent(rq.ComponentId);
            if (cp == null)
            {
                cb(RpcResult.Failure);
                return;
            }

            cp.OnMessageAsync(new NetReqMsg(rp.Ops, rp.Data), cb);
        }

        public void OnPush(RpcPush rp, Action<bool> cb)
        {
            var entity = rp.ObjId != 0 ? Es.Get(rp.ObjId) : this;
            if (entity == null)
            {
                cb(false);
                return;
            }
            if (rp.ComponentId == 0)
            {
                entity.OnMessageAsync(new NetPushMsg(rp.Ops, rp.Data), cb);
                return;
            }

            var cp = entity.GetComponent(rp.ComponentId);
            if (cp == null)
            {
                cb(false);
                return;
            }

            cp.OnMessageAsync(new NetReqMsg(rp.Ops, rp.Data), cb);
        }

#endif
    }

    public abstract class FlushablePlayer : Player, IFlushable
    {
        public PersistSys Ps { get; private set; }
        private Func<IAsyncRedisClient> _redisClientGetter;
        private IAsyncRedisClient RedisClient
        {
            get { return _redisClientGetter(); }
        }

        public async void Flush()
        {
            if (Ps != null)
                await Ps.PersistAsync(RedisClient);

            if (Ss != null)
                Ss.Sync(Session);
        }

        protected override void OnInit(ObjArg arg)
        {
            base.OnInit(arg);
            var more = arg.As<FlushablePlayerArg>();
            if (!more.PersistEnabled) return;

            _redisClientGetter = more.RedisClientGetter;
            Ps = New<PersistSys>(new DataSysArg(this, Es));
        }

        protected override void OnStart()
        {
            base.OnStart();
            if (Ps != null)
                Ps.Start();
        }

        protected override void OnDestroy()
        {
            if (Ps != null)
                Ps.Destroy();

            base.OnDestroy();
        }
    }
}
