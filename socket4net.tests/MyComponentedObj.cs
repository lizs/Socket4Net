using NUnit.Framework;

namespace socket4net.tests
{
    internal enum EComponentId
    {
        MyComponent,
        MyDerivedComponent,
    }

    [ComponentId((short)EComponentId.MyComponent)]
    internal class MyComponent : Component<EProperty>
    {
        internal MyComponentedObj Host
        {
            get { return GetAncestor<MyComponentedObj>(); }
        }

        protected override void OnInjectProperties()
        {
            base.OnInjectProperties();
            Host.Inject(new SettableBlock<EProperty, float>(EProperty.One, 1.0f, EBlockMode.Temporary));
        }

        public override void OnPropertyChanged(IBlock<EProperty> block)
        {
            base.OnPropertyChanged(block);
            switch (block.Id)
            {
                case EProperty.One:
                {
                    //Assert.AreEqual(Host.Get<float>(EProperty.One), 2.0f);
                    break;
                }
            }
        }

        protected override void OnStart()
        {
            base.OnStart();
            Assert.AreEqual(Host.Get<float>(EProperty.One), 1.0f);

            //Host.Set(EProperty.One, 2.0f);
            //Assert.AreEqual(Host.Get<float>(EProperty.One), 2.0f);
        }
    }

    [ComponentId((short)EComponentId.MyDerivedComponent)]
    [DependOn(typeof(MyComponent))]
    internal class MyDerivedComponent : MyComponent
    {
    }

    [ConsistsOf(typeof (MyComponent), typeof (MyDerivedComponent))]
    internal class MyComponentedObj : ComponentedObj<EProperty>
    {
        protected override void OnInit(ObjArg objArg)
        {
            base.OnInit(objArg);

            var cp1 = GetComponent<MyComponent>();
            Assert.NotNull(cp1);
            Assert.True(cp1.Initialized);

            var cp2 = GetComponent<MyDerivedComponent>();
            Assert.NotNull(cp2);
            Assert.True(cp2.Initialized);
        }

        protected override void OnStart()
        {
            base.OnStart();

            var cp = GetComponent<MyComponent>();
            Assert.NotNull(cp);
            Assert.True(cp.Started);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            var cp = GetComponent<MyComponent>();
            Assert.Null(cp);
        }
    }

    internal class MyDerivedComponentedObj : MyComponentedObj{}
}