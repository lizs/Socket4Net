using System;

namespace Pi.Core.Business.Base.New
{
    /// <summary>
    ///     基础对象
    /// </summary>
    public abstract class Obj : IComparable<Obj>
    {
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
        ///     比较两对象
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public virtual int CompareTo(Obj other)
        {
            return InstanceId.CompareTo(other.InstanceId);
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
        ///     初始
        /// </summary>
        public void Init()
        {
            InstanceId = GetInstanceId();
        }

        /// <summary>
        ///     启动
        /// </summary>
        public abstract void Start();

        /// <summary>
        ///     销毁
        /// </summary>
        public abstract void Destroy();

        /// <summary>
        ///     重置
        /// </summary>
        public abstract void Reset();

        /// <summary>
        ///     在对象初始之后调用
        /// </summary>
        public virtual void OnInit()
        {
        }

        /// <summary>
        ///     在对象启动之后调用
        /// </summary>
        public virtual void OnStart()
        {
        }

        /// <summary>
        ///     在对象销毁之前调用
        /// </summary>
        public virtual void OnDestroy()
        {
        }

        /// <summary>
        ///     在重置之后调用
        /// </summary>
        public virtual void OnReset()
        {
        }

        /// <summary>
        ///     实例id种子
        /// </summary>
        private static int _seed;
        protected static int GetInstanceId()
        {
            return ++_seed;
        }
    }
}
