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

namespace node
{
    public class NodesMgrArg : UniqueMgrArg
    {
        public LauncherConfig Config { get; private set; }
        public NodesMgrArg(IObj parent, LauncherConfig config)
            : base(parent)
        {
            Config = config;
        }
    }

    /// <summary>
    ///     节点管理器
    /// </summary>
    public abstract class NodesMgr : UniqueMgr<Guid, Node>
    {
        public LauncherConfig Config { get; private set; }
        private RedisMgr<AsyncRedisClient> _redisMgr;

        protected override void OnInit(ObjArg arg)
        {
            base.OnInit(arg);

            var more = arg.As<NodesMgrArg>();
            Config = more.Config;
            if (Config == null)
                throw new ArgumentNullException("arg");

            Logger.Ins.Info(Config.ToString());

            // 创建服务器
            if (!CreateNodes())
                throw new Exception("创建服务器失败");
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _redisMgr.Destroy();
        }

        public ServerNode<TSession> GetServer<TSession>() where TSession : class, IRpcSession, new()
        {
            return GetFirst<ServerNode<TSession>>();
        }

        public ClientNode<TSession> GetClient<TSession>() where TSession : class, IRpcSession, new()
        {
            return GetFirst<ClientNode<TSession>>();
        }

        public Node<TSession> GetNode<TSession>() where TSession : class, IRpcSession, new()
        {
            return GetFirst<Node<TSession>>();
        }

        public IEnumerable<T> GetSession<T>() where T : class, IRpcSession, new()
        {
            return GetFirst<Node<T>>().GetSession<T>();
        }

        public T GetSession<T>(long sid) where T : class, IRpcSession, new()
        {
            return GetFirst<Node<T>>().GetSession<T>(sid);
        }

        public IEnumerable<T> GetSession<T>(Predicate<T> condition) where T : class, IRpcSession, new()
        {
            return GetFirst<Node<T>>().GetSession<T>(condition);
        }

        public T GetFirstSession<T>(Predicate<T> condition) where T : class, IRpcSession, new()
        {
            return GetFirst<Node<T>>().GetFirstSession<T>(condition);
        }

        public T GetFirstSession<T>() where T : class, IRpcSession, new()
        {
            return GetFirst<Node<T>>().GetFirstSession<T>();
        }

        private bool CreateNodes()
        {
            if (Config == null) return false;

            _redisMgr = New<RedisMgr<AsyncRedisClient>>(new RedisMgrArg(this,
                Config.RedisNodes), true);

            foreach (var cfg in Config.Servers)
            {
                Create(cfg.NodeClass ?? typeof(ServerNode<>).MakeGenericType(new[] { MapSession(cfg.Type) }),
                    new NodeArg(this, cfg.Guid, cfg), false);
            }

            foreach (var cfg in Config.Clients)
            {
                Create(cfg.NodeClass ?? typeof(ClientNode<>).MakeGenericType(new[] { MapSession(cfg.Type) }),
                    new NodeArg(this, cfg.Guid, cfg), false);
            }

            return true;
        }

        protected abstract Type MapSession(string type);
    }
}
