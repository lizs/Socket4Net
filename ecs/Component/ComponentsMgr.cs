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

namespace ecs
{
    public class ComponentsMgr : UniqueMgr<short, Component>
    {
        private void AddDependencies<T>() where T : Component, new()
        {
            AddDependencies(typeof(T));
        }

        private void AddDependencies(Type cpType)
        {
            var targets = ComponentDepedencyCache.Ins.Get(cpType);
            if (targets.IsNullOrEmpty()) return;

            foreach (var target in targets)
            {
                CreateComponent(target);
            }
        }

        private Component CreateComponent(Type cpType)
        {
            var id = ComponentIdCache.Ins.Get(cpType);
            if(Exist(id)) return Get(id);

            var cp = (Component)Create(cpType, new ComponentArg(this, id));
            Add(cp);
            return cp;
        }

        private T CreateComponent<T>() where T : Component, new()
        {
            var id = ComponentIdCache.Ins.Get(typeof(T));
            return Exist(id) ? Get<T>(id) : Create<T>(new ComponentArg(this, id));
        }

        public T AddComponent<T>() where T : Component, new()
        {
            var id = ComponentIdCache.Ins.Get(typeof(T));
            if (Exist(id)) return Get<T>(id);

            AddDependencies<T>();
            return CreateComponent<T>();
        }

        public Component AddComponent(Type cpType)
        {
            var id = ComponentIdCache.Ins.Get(cpType);
            if (Exist(id)) return Get(id);

            AddDependencies(cpType);
            return CreateComponent(cpType);
        }

        public bool ExistComponent<T>() where T : Component
        {
            return ExistComponent(typeof (T));
        }

        public bool ExistComponent(Type cpType)
        {
            var id = ComponentIdCache.Ins.Get(cpType);
            return ExistComponent(id);
        }

        public bool ExistComponent(short cpId)
        {
            return Exist(cpId);
        }
    }
}
