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
using System.Linq;
using System.Threading.Tasks;
using socket4net;

namespace ecs
{
    public enum ESyncOps
    {
        Sync,
    }

    [ComponentId((short)EInternalComponentId.Sync)]
    public class SyncComponent : RpcComponent
    {
        private EntitySys _es;
        private EntitySys Es
        {
            get { return _es ?? (_es = GetAncestor<Player>().Es); }
        }

        public override Task<bool> OnPush(short ops, byte[] data)
        {
            switch ((ESyncOps) ops)
            {
                case ESyncOps.Sync:
                {
                    var lst = PiSerializer.DeserializeValue<List<EntityProto>>(data);
                    if (lst.IsNullOrEmpty()) return Task.FromResult(false);
                    foreach (var proto in lst)
                    {
                        if (proto is EntityDestroyProto)
                        {
                            Es.Destroy(proto.Id);
                        }
                        else if (proto is EntityUpdateProto)
                        {
                            var update = proto as EntityUpdateProto;
                            var entity = Es.Get(update.Id) ??
                                         Es.Create<Entity>(Type.GetType(string.Format("{0},Shared", update.Type)), new EntityArg(Es, update.Id), true);

                            entity.Apply(update.Blocks.Select(x => new Pair<short, byte[]>(x.Pid, x.Data)));
                        }
                    }

                    return Task.FromResult(true);
                }
            }

            return Task.FromResult(false);
        }
    }
}
