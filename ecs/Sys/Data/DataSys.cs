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

namespace ecs
{
    public class DataSysArg : ObjArg
    {
        public DataSysArg(IObj owner, EntitySys es) : base(owner)
        {
            Es = es;
        }

        public EntitySys Es { get; private set; }
    }

    /// <summary>
    ///     数据系统
    /// </summary>
    public abstract class DataSys : Obj
    {
        protected EntitySys Es;
        protected readonly Dictionary<long, Pair<Type, List<IBlock>>> UpdateCache = new Dictionary<long, Pair<Type, List<IBlock>>>();
        protected readonly Dictionary<long, Type> DestroyCache = new Dictionary<long, Type>(); 

        protected override void OnInit(ObjArg arg)
        {
            base.OnInit(arg);

            Es = arg.As<DataSysArg>().Es;

            // 监听实体系统
            Es.GlobalListen(OnEntityPropertyChanged);
            Es.EventDefaultObjCreated += OnEntityCreated;
            Es.EventObjDestroyed += OnEntityDestroyed;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Es.GlobalUnlisten(OnEntityPropertyChanged);
            Es.EventDefaultObjCreated -= OnEntityCreated;
            Es.EventObjDestroyed -= OnEntityDestroyed;
        }

        private void OnEntityCreated(Entity entity)
        {
            CacheBlock(entity.Id, entity.GetType(), entity.Blocks);
        }

        private void OnEntityDestroyed(long id, Type type)
        {
            if(!DestroyCache.ContainsKey(id))
                DestroyCache.Add(id, type);

            UpdateCache.Remove(id);
        }

        private void CacheBlock(long id, Type type, IReadOnlyCollection<IBlock> blocks)
        {
            if(blocks.IsNullOrEmpty()) return;
            foreach (var block in blocks)
            {
                CacheBlock(id, type, block);
            }
        }

        private void CacheBlock(long id, Type type, IBlock block)
        {
            if (!UpdateCache.ContainsKey(id))
                UpdateCache[id] = new Pair<Type, List<IBlock>>(type, new List<IBlock> {block});

            var lst = UpdateCache[id];
            if (lst.Value.Contains(block)) return;
            lst.Value.Add(block);
        }

        private void OnEntityPropertyChanged(Entity entity, IBlock block)
        {
            CacheBlock(entity.Id, entity.GetType(), block);
        }
    }
}
