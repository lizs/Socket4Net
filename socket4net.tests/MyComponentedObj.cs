using Xunit;

namespace socket4net.tests
{
    internal enum EComponentId
    {
        MyComponent,
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

        [Fact]
        public override void OnPropertyChanged(IBlock<EProperty> block)
        {
            base.OnPropertyChanged(block);
            switch (block.Id)
            {
                case EProperty.One:
                {
                    Assert.Equal(Host.Get<float>(EProperty.One), 2.0f);
                    break;
                }
            }
        }

        [Fact]
        protected override void OnStart()
        {
            base.OnStart();
            Assert.Equal(Host.Get<float>(EProperty.One), 1.0f);

            Host.Set(EProperty.One, 2.0f);
            Assert.Equal(Host.Get<float>(EProperty.One), 2.0f);
        }
    }

    internal class MyComponentedObj : ComponentedObj<EProperty>
    {
        protected override void SpawnComponents()
        {
            base.SpawnComponents();
            AddComponent<MyComponent>();
        }

        protected override void OnInit(ObjArg objArg)
        {
            base.OnInit(objArg);

            var cp = GetComponent<MyComponent>();
            Assert.NotNull(cp);
            Assert.True(cp.Initialized);
        }

        [Fact]
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
}