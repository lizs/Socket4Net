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
    public class RedisMgrArg : UniqueMgrArg
    {
        public IEnumerable<NodeElement> Config { get; private set; }
        public RedisMgrArg(IObj parent, IEnumerable<NodeElement> cfg)
            : base(parent)
        {
            Config = cfg;
        }
    }

    public class RedisMgr<T> : UniqueMgr<string, T> where T : RedisClient, new()
    {
        public static RedisMgr<T> Instance { get; private set; }
        public RedisMgr()
        {
            if(Instance != null) throw new Exception("RedisMgr already instantiated!");
            Instance = this;
        }

        public IEnumerable<NodeElement> Config { get; private set; }

        protected override void OnInit(ObjArg arg)
        {
            base.OnInit(arg);

            var more = arg.As<RedisMgrArg>();
            Config = more.Config;

            foreach (var redisElement in Config)
            {
                Create<T>(new RedisClientArg(this, redisElement.Type, redisElement), false);
            }
        }
    }
}
