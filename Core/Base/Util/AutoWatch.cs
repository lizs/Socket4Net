using System;
using System.Diagnostics;

namespace socket4net
{
    /// <summary>
    ///     自动计时器
    /// </summary>
    public class AutoWatch : IDisposable
    {
        private readonly Stopwatch _watch = new Stopwatch();
        public AutoWatch(string name, long threhold = 50, Action<string, long> handler = null)
        {
            Name = name;
            Handler = handler;
            Threhold = threhold;
            _watch.Start();
        }

        public string Name { get; private set; }
        public long Threhold { get; private set; }
        public Action<string, long> Handler { get; private set; }

        public void Dispose()
        {
            _watch.Stop();
            if (_watch.ElapsedMilliseconds > Threhold)
                Logger.Instance.WarnFormat("{0} : {1} ms", Name, _watch.ElapsedMilliseconds);

            if (Handler != null)
                Handler(Name, _watch.ElapsedMilliseconds);
        }

        public long ElapsedMilliseconds { get { return _watch.ElapsedMilliseconds; } }
    }
}
