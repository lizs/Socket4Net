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
using System.Collections.Generic;
using ecs;
using socket4net;

namespace Shared
{
    public class Ship : Entity
    {
        protected override void OnInit(ObjArg arg)
        {
            base.OnInit(arg);

            // 注入属性
            Inject(new []
            {
                BlockMaker.Create(EPid.One),
                BlockMaker.Create(EPid.Two),
                BlockMaker.Create(EPid.Three),
            });
        }

        protected override void OnReset()
        {
            base.OnReset();

            One = 1;
            Two = 2;
            Three = new List<int>{1, 2, 3};
        }

        protected override void OnStart()
        {
            base.OnStart();
            Logger.Ins.Info("{0} started!", Name);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Logger.Ins.Info("{0} destroyed!", Name);
        }

        protected override void SpawnComponents()
        {
            base.SpawnComponents();
            AddComponent<PropertyComponent>();
        }

        public int One
        {
            get { return Get<int>((short)EPid.One); }
            set { Set((short)EPid.One, value); }
        }

        public int Two
        {
            get { return Get<int>((short)EPid.Two); }
            set { IncTo((short)EPid.Two, value); }
        }

        public List<int> Three
        {
            get { return GetList<int>((short)EPid.Three); }
            set
            {
                RemoveAll((short)EPid.Three);
                AddRange((short) EPid.Three, value);
            }
        }
    }
}
