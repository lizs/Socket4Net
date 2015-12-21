using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace socket4net.tests
{
    internal class PropertiedObjCase : Case
    {
        [Test]
        public void Do()
        {
            var obj = Obj.Create<MyPropertiedObj>(new PropertiedObjArg<EProperty>(null, 1, null));
            Assert.IsEmpty(obj.Blocks);

            obj.Reset();
            Assert.True(obj.Reseted);
            Assert.True(obj.Blocks.Count() == 3);
            Assert.AreEqual(obj.Get<int>(EProperty.One), 1);
            Assert.AreEqual(obj.GetList<int>(EProperty.Two), new List<int>() { 1, 2, 3 });
            Assert.AreEqual(obj.Get<float>(EProperty.Three), 3.0f);

            obj.Set(EProperty.One, 2);
            Assert.AreEqual(obj.Get<int>(EProperty.One), 2);

            obj.Add(EProperty.Two, 4);
            Assert.AreEqual(obj.GetList<int>(EProperty.Two), new List<int>(){1, 2, 3, 4});
            obj.Insert(EProperty.Two, 3, 5);
            Assert.AreEqual(obj.GetList<int>(EProperty.Two), new List<int>() { 1, 2, 3, 5, 4 });
            obj.Swap<int>(EProperty.Two, 3, 4);
            Assert.AreEqual(obj.GetList<int>(EProperty.Two), new List<int>() { 1, 2, 3, 4, 5 });
            obj.AddRange(EProperty.Two, new List<int> {6, 7});
            Assert.AreEqual(obj.GetList<int>(EProperty.Two), new List<int> {1, 2, 3, 4, 5, 6, 7});
            obj.Replace(EProperty.Two, 0, 0);
            Assert.AreEqual(obj.GetList<int>(EProperty.Two), new List<int> { 0, 2, 3, 4, 5, 6, 7 });
            obj.Remove(EProperty.Two, 0);
            Assert.AreEqual(obj.GetList<int>(EProperty.Two), new List<int> { 2, 3, 4, 5, 6, 7 });

            obj.Inc(EProperty.Three, 1.0f);
            Assert.AreEqual(obj.Get<float>(EProperty.Three), 4.0f);
            obj.IncTo(EProperty.Three, 5.0f);
            Assert.AreEqual(obj.Get<float>(EProperty.Three), 5.0f);
            obj.IncTo(EProperty.Three, -1.0f);
            Assert.AreEqual(obj.Get<float>(EProperty.Three), 0.0f);
            obj.IncTo(EProperty.Three, 11.0f);
            Assert.AreEqual(obj.Get<float>(EProperty.Three), 10.0f);
        }
    }
}