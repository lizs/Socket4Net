using System;

namespace socket4net
{
    /// <summary>
    ///     接口仅作代码组织用
    /// </summary>
    public interface IObj
    {
        int InstanceId { get; }

        /// <summary>
        ///     名字
        /// </summary>
       string Name { get; }

        /// <summary>
        ///     调度优先级（类似Unity中的Layer）
        /// </summary>
        int Priority { get; }

        /// <summary>
        ///     比较
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        int CompareTo(IObj other);

        void Init();
        void Reset();
        void Start();
        void AfterStart();
        void Destroy();
        void SetArgument(ObjArg arg);
    }

    public abstract class ObjArg
    {
        public static ObjArg Empty
        {
            get { return new EmptyArg(); }
        }
    }

    /// <summary>
    ///     空参数
    /// </summary>
    public class EmptyArg : ObjArg
    {
    }

    public abstract class Obj : IObj
    {
        public enum Flag
        {
            Empty,

            BeforeReset = 1 << 0,
            OnReset = 1 << 1,
            AfterReset = 1 << 2,

            BeforeInit = 1 << 3,
            OnInit = 1 << 4,
            AfterInit = 1 << 5,

            BeforeStart = 1 << 6,
            OnStart = 1 << 7,
            AfterStart = 1 << 8,

            BeforeDestroy = 1 << 9,
            OnDestroy = 1 << 10,
            AfterDestroy = 1 << 11,
        }

        public Flag InvokeFlag { get; private set; }

        /// <summary>
        ///     参数
        /// </summary>
        public ObjArg Argument { get; private set; }
        public virtual void SetArgument(ObjArg arg)
        {
            Argument = arg;
        }

        /// <summary>
        ///     实例动态id
        ///     仅运行时唯一
        /// </summary>
        public int InstanceId { get; private set; }

        /// <summary>
        ///     名字
        /// </summary>
        public virtual string Name
        {
            get { return string.Format("{0}:{1}", GetType().FullName, InstanceId); }
        }

        /// <summary>
        ///     调度优先级（类似Unity中的Layer）
        /// </summary>
        public virtual int Priority
        {
            get { return 0; }
        }

        /// <summary>
        /// 附带自定义数据
        /// </summary>
        protected object UserData { get; set; }
        public T GetUserData<T>()
        {
            return (T) UserData;
        }

        /// <summary>
        /// 设置自定义数据
        /// </summary>
        /// <param name="obj"></param>
        public void SetUserData(object obj)
        {
            UserData = obj;
        }

        /// <summary>
        ///     赋予运行时实例id
        /// </summary>
        protected Obj()
        {
            InstanceId = GenInstanceId();
        }

        /// <summary>
        ///     是否已初始化
        /// </summary>
        public bool Initialized { get; protected set; }

        /// <summary>
        ///     是否已启动
        /// </summary>
        public bool Started { get; private set; }

        /// <summary>
        ///     是否已销毁
        /// </summary>
        public bool Destroyed { get; private set; }

        /// <summary>
        ///     是否重置过
        /// </summary>
        public bool Reseted { get; private set; }

        /// <summary>
        ///     根据优先级比较两对象
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public virtual int CompareTo(IObj other)
        {
            return Priority.CompareTo(other.Priority);
        }

        /// <summary>
        ///     重写ToString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name;
        }


        /// <summary>
        ///     设置标记
        /// </summary>
        /// <param name="flag"></param>
        private void SetFlag(Flag flag)
        {
            if (InvokeFlag != Flag.Empty && (InvokeFlag & ~flag) == Flag.Empty)
                throw new Exception(string.Format("{0} already invoked!", flag));

            InvokeFlag |= flag;
        }

        /// <summary>
        ///     初始
        /// </summary>
        public void Init()
        {
            BeforeInit();

            // 执行初始化
            OnInit();

            // 标识初始完毕
            Initialized = true;

            // 初始完毕
            AfterInit();
        }

        protected virtual void BeforeInit()
        {
            SetFlag(Flag.BeforeInit);
        }

        protected virtual void OnInit()
        {
            SetFlag(Flag.OnInit);
        }

        protected virtual void AfterInit()
        {
            SetFlag(Flag.AfterInit);
        }

        /// <summary>
        ///     启动
        /// </summary>
        public void Start()
        {
            if(!Initialized)
                throw new Exception(string.Format("{0} Not initialized yet!", Name));

            BeforeStart();
            OnStart();
            Started = true;
            //AfterStart();
        }

        /// <summary>
        ///     在启动之前调用
        /// </summary>
        protected virtual void BeforeStart()
        {
            SetFlag(Flag.BeforeStart);
        }

        /// <summary>
        ///     执行启动
        /// </summary>
        protected virtual void OnStart()
        {
            SetFlag(Flag.OnStart);
        }

        /// <summary>
        ///     在对象启动之后调用
        /// </summary>
        public virtual void AfterStart()
        {
            SetFlag(Flag.AfterStart);
        }

        /// <summary>
        ///     销毁
        /// </summary>
        public void Destroy()
        {
            BeforeDestroy();
            OnDestroy();
            Destroyed = true;
            AfterDestroy();
        }

        protected virtual void BeforeDestroy()
        {
            SetFlag(Flag.BeforeDestroy);
        }

        protected virtual void AfterDestroy()
        {
            SetFlag(Flag.AfterDestroy);
        }

        protected virtual void OnDestroy()
        {
            SetFlag(Flag.OnDestroy);
        }

        /// <summary>
        ///     重置
        /// </summary>
        public void Reset()
        {
            BeforeReset();
            OnReset();
            Reseted = true;
            AfterReset();
        }

        protected virtual void BeforeReset()
        {
            SetFlag(Flag.BeforeReset);
        }

        protected virtual void OnReset()
        {
            SetFlag(Flag.OnReset);
        }

        protected virtual void AfterReset()
        {
            SetFlag(Flag.AfterReset);
        }

        /// <summary>
        ///     实例id种子
        /// </summary>
        private static int _seed;
        protected static int GenInstanceId()
        {
            return ++_seed;
        }
    }
}