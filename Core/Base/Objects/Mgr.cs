using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace socket4net
{
    /// <summary>
    ///     对象管理器
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public class Mgr<TValue> : Obj, IEnumerable<TValue> where TValue : Obj
    {
        private readonly List<TValue> _items = new List<TValue>();

        protected override void OnDestroy()
        {
            base.OnDestroy();
            foreach (var item in this.Reverse())
                item.Destroy();

            _items.Clear();
        }

        protected override void OnStart()
        {
            base.OnStart();
            foreach (var item in this)
                item.Start();
        }

        protected override void OnReset()
        {
            base.OnReset();
            foreach (var item in this)
                item.Reset();
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        public void Destroy<T>() where T : TValue
        {
            var victims = Get<T>();
            _items.RemoveAll(value => value is T);
            foreach (var obj in victims)
                obj.Destroy();
        }

        public void Destroy<T>(Predicate<T> condition) where T : TValue
        {
            var victims = Get<T>(condition);
            _items.RemoveAll(x => x is T && condition(x as T));
            foreach (var obj in victims)
                obj.Destroy();
        }

        public List<T> Get<T>() where T : TValue
        {
            return this.OfType<T>().ToList(); 
        }

        public List<T> Get<T>(Predicate<T> condition) where T : TValue
        {
            return this.OfType<T>().Where(x=>condition(x)).ToList();
        }

        public List<T> GetReverse<T>() where T : TValue
        {
            return this.OfType<T>().Reverse().ToList();
        }

        public T GetFirst<T>() where T : TValue
        {
            return Get<T>().FirstOrDefault();
        }

        public new T Create<T>(ObjArg arg, bool start) where T : class, TValue, new()
        {
            var ret = Obj.Create<T>(arg, start);
            _items.Add(ret);
            return ret;
        }
    }
}