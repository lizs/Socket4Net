using System.Collections.Generic;

namespace socket4net.tests
{
    internal enum EProperty
    {
        One,
        Two,
        Three,
    }

    internal class MyPropertiedObj : PropertiedObj<EProperty>
    {
        protected override void OnReset()
        {
            base.OnReset();

            Inject(new SettableBlock<EProperty, int>(EProperty.One, 1, EBlockMode.Temporary));
            Inject(new ListBlock<EProperty, int>(EProperty.Two, new List<int> {1, 2, 3}, EBlockMode.Temporary));
            Inject(new IncreasableBlock<EProperty, float>(EProperty.Three, 3.0f, EBlockMode.Temporary, .0f, 10.0f));
        }
    }
}