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
using System.Collections;
#if NET45
using System.Threading.Tasks;
#endif

namespace socket4net
{
    /// <summary>
    ///     Arguments used to initialize an Obj
    /// </summary>
    public class ObjArg
    {
        /// <summary>
        ///     Owner of to be initialized Obj
        /// </summary>
        public IObj Owner { get; private set; }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="owner"></param>
        public ObjArg(IObj owner)
        {
            Owner = owner;
        }

        /// <summary>
        ///     Empty obj argument
        /// </summary>
        public static ObjArg Empty => new EmptyArg();

        /// <summary>
        ///     Cast this arg to T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T As<T>() where T : ObjArg
        {
            if (!(this is T))
                throw new ArgumentException("ObjArg's type isn't " + typeof(T).Name);

            return (T) this;
        }
    }

    /// <summary>
    ///     Empty obj argument
    /// </summary>
    public class EmptyArg : ObjArg
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public EmptyArg() : base(null)
        {
        }
    }

    /// <summary>
    ///     Interface Obj
    /// </summary>
    public interface IObj
    {
        /// <summary>
        ///     Obj instance id
        ///     Unique only before current process dead
        /// </summary>
        int InstanceId { get; }

        /// <summary>
        ///     Owner
        /// </summary>
        IObj Owner { get; }

        /// <summary>
        ///     name
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Schedule priority
        ///     Just like Unity's layer
        /// </summary>
        int Priority { get; }

        /// <summary>
        ///     Owner description
        /// </summary>
        string OwnerDescription { get; }

        /// <summary>
        ///     If initialized
        /// </summary>
        bool Initialized { get; }

        /// <summary>
        ///     If started
        /// </summary>
        bool Started { get; }

        /// <summary>
        ///     If destroyed
        /// </summary>
        bool Destroyed { get; }

        /// <summary>
        ///     If IObj's 'Born' invoked 
        /// </summary>
        bool Fresh { get; }

        /// <summary>
        ///     comparer
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        int CompareTo(IObj other);

        /// <summary>
        ///     Initialize
        /// </summary>
        /// <param name="arg"></param>
        void Init(ObjArg arg);

        /// <summary>
        ///     Born
        ///     Obj only born once during it's life circle
        /// </summary>
        void Born();

        /// <summary>
        ///     Run
        /// </summary>
        void Start();

        /// <summary>
        ///     Destroy
        /// </summary>
        void Destroy();

        /// <summary>
        ///     Get obj's ancestor
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T GetAncestor<T>() where T : class ,IObj;

        /// <summary>
        ///     Get user data
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T GetUserData<T>();

        /// <summary>
        ///     Set user data
        /// </summary>
        /// <param name="obj"></param>
        void SetUserData(object obj);

        #region Coroutine interfaces
        /// <summary>
        ///     Create an IEnumerator used in Coroutine scheduler
        ///     to waiting for 'n' ms
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        IEnumerator WaitFor(uint n);

        /// <summary>
        ///     Stop 'coroutine'
        /// </summary>
        /// <param name="coroutine"></param>
        void StopCoroutine(Coroutine coroutine);

        /// <summary>
        ///     Start 'fun' as an coroutine
        /// </summary>
        /// <param name="fun"></param>
        /// <returns></returns>
        Coroutine StartCoroutine(Func<IEnumerator> fun);

        /// <summary>
        ///     Start 'fun' as an coroutine
        /// </summary>
        /// <param name="fun"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        Coroutine StartCoroutine(Func<object[], IEnumerator> fun, params object[] args);
        #endregion

        #region Timer interfaces
        /// <summary>
        ///     clear timers attached on this obj
        /// </summary>
        void ClearTimers();

        /// <summary>
        ///     Excute 'action' after 'delay' ms for every 'period' ms
        /// </summary>
        void InvokeRepeating(Action action, uint delay, uint period);

        /// <summary>
        ///     Excute 'action' after 'delay' ms
        /// </summary>
        /// <param name="action"></param>
        /// <param name="delay"></param>
        void Invoke(Action action, uint delay);

        /// <summary>
        ///     Excute 'action' when 'when'
        /// </summary>
        /// <param name="action"></param>
        /// <param name="when"></param>
        void Invoke(Action action, DateTime when);

        /// <summary>
        ///     Excute 'action' every 'hour:min:s'
        /// </summary>
        /// <param name="action"></param>
        /// <param name="hour"></param>
        /// <param name="min"></param>
        /// <param name="s"></param>
        void Invoke(Action action, int hour, int min, int s);

        /// <summary>
        ///     Excute 'action' everyday's 'time' clock
        /// </summary>
        /// <param name="action"></param>
        /// <param name="time"></param>
        void Invoke(Action action, TimeSpan time);

        /// <summary>
        ///     Cancel 'action'
        /// </summary>
        /// <param name="action"></param>
        void CancelInvoke(Action action);

#if NET45
        /// <summary>
        ///     Excute 'action' everyday's 'times' clock
        /// </summary>
        Task<bool> InvokeAsync(Action action, params TimeSpan[] times);

        /// <summary>
        ///     Excute 'action' when 'whens'
        /// </summary>
        Task<bool> InvokeAsync(Action action, params DateTime[] whens);

        /// <summary>
        ///     Excute 'action' everyday's 'time' clock
        /// </summary>
        /// <param name="action"></param>
        /// <param name="time"></param>
        Task<bool> InvokeAsync(Action action, TimeSpan time);

        /// <summary>
        ///     Excute 'action' when 'when'
        /// </summary>
        /// <param name="action"></param>
        /// <param name="when"></param>
        Task<bool> InvokeAsync(Action action, DateTime when);

        /// <summary>
        ///     Excute 'action' after 'delay' ms
        /// </summary>
        /// <param name="action"></param>
        /// <param name="delay"></param>
        Task<bool> InvokeAsync(Action action, uint delay);
#endif
        #endregion
    }

    public partial class Obj : IObj
    {
        private object _userData;
        private int InstanceId { get; }

        /// <summary>
        ///     Obj instance id
        ///     Unique only before current process dead
        /// </summary>
        int IObj.InstanceId => InstanceId;

        /// <summary>
        ///     name
        /// </summary>
        public virtual string Name => GetType().FullName;

        /// <summary>
        ///     Schedule priority
        ///     Just like Unity's layer
        /// </summary>
        public virtual int Priority => 0;

        T IObj.GetUserData<T>()
        {
            return (T)_userData;
        }

        void IObj.SetUserData(object obj)
        {
            _userData = obj;
        }

        /// <summary>
        ///     Constructor
        ///     InstanceId is assigned here
        /// </summary>
        protected Obj()
        {
            InstanceId = GenInstanceId();
        }

        /// <summary>
        ///     Owner
        /// </summary>
        public IObj Owner { get; set; }

        /// <summary>
        ///     Owner description
        /// </summary>
        public string OwnerDescription => Owner != null ? Owner.Name : "null";

        /// <summary>
        ///     If initialized
        /// </summary>
        public bool Initialized { get; private set; }

        /// <summary>
        ///     If started
        /// </summary>
        public bool Started { get; private set; }

        /// <summary>
        ///     If destroyed
        /// </summary>
        public bool Destroyed { get; private set; }

        /// <summary>
        ///     If IObj's 'Born' invoked 
        /// </summary>
        public bool Fresh { get; private set; }

        /// <summary>
        ///     comparer
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public virtual int CompareTo(IObj other)
        {
            return Priority.CompareTo(other.Priority);
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        ///     Create an obj of type 'T' with arg
        ///     If 'start' == true, Obj will be started after initialized
        /// </summary>
        /// <returns></returns>
        public static T New<T>(ObjArg arg, bool start = false) where T : IObj, new()
        {
            var ret = new T();
            ret.Init(arg);
            if (start)
                ret.Start();
            return ret;
        }

        /// <summary>
        ///     Create an obj of 'type' with arg
        ///     If 'start' == true, Obj will be started after initialized
        /// </summary>
        /// <returns></returns>
        public static IObj New(Type type, ObjArg arg, bool start = false)
        {
            return ObjFactory.Create(type, arg, start);
        }

        /// <summary>
        ///     Create an obj of 'type' with arg and return 'obj as T'
        ///     If 'start' == true, Obj will be started after initialized
        /// </summary>
        /// <returns></returns>
        public static T New<T>(Type type, ObjArg arg, bool start = false) where T : class, IObj
        {
            return New(type, arg, start) as T;
        }

        /// <summary>
        ///     Initialize
        /// </summary>
        /// <param name="arg"></param>
        public void Init(ObjArg arg)
        {
            if(Initialized)
                throw new Exception("Already initialized");

            // 执行初始化
            OnInit(arg);
            Initialized = true;
        }

        /// <summary>
        ///     Run
        /// </summary>
        public void Start()
        {
            if(!Initialized)
                throw new Exception("Not initialized yet!");

            if (Started)
            {
                Logger.Ins.Warn("Already started");
                return;
            }

            OnStart();
            Started = true;
        }

        /// <summary>
        ///     Invoked when obj started
        /// </summary>
        protected virtual void OnStart()
        {
        }

        /// <summary>
        ///     Destroy
        /// </summary>
        public void Destroy()
        {
            if (Destroyed)
            {
                Logger.Ins.Warn("Already destroyed");
                return;
            }

            OnDestroy();
            Destroyed = true;
        }

        /// <summary>
        ///     Born
        ///     Obj only born once during it's life circle
        /// </summary>
        public void Born()
        {
            if (Fresh)
            {
                Logger.Ins.Warn("Already born");
                return;
            }

            OnBorn();
            Fresh = true;
        }

        /// <summary>
        ///     Invoked when obj born
        /// </summary>
        protected virtual void OnBorn()
        {
        }

        /// <summary>
        ///     Get obj's ancestor
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetAncestor<T>() where T : class, IObj
        {
            if (this is T) return (this as T);
            if (Owner == null) return null;
            var owner = Owner as T;
            return owner ?? Owner.GetAncestor<T>();
        }

        private static int _seed;
        private static int GenInstanceId()
        {
            return ++_seed;
        }
    }
}