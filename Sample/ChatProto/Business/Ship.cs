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
