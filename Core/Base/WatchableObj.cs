using System;

namespace socket4net
{
    public interface IWatchable<T>
    {
        event Func<T, bool> Watch;
    }

    public class Watch<T>
    {
        public IWatchable<T> Target { get; private set; }
        public Func<T, bool> Callback { get; private set; }
        public Watch(IWatchable<T> target, Func<T, bool> cb)
        {
            Target = target;
            Callback = cb;

            Target.Watch += Callback;
        }

        public void Destroy()
        {
            Target.Watch -= Callback;
        }
    }
}