using System;

namespace socket4net
{
    public class LauncherArg : ObjArg
    {
        public LauncherArg(ILog logger = null, bool passiveLogicServiceEnabled = false) : base(null)
        {
            Logger = logger;
            PassiveLogicServiceEnabled = passiveLogicServiceEnabled;
        }

        public bool PassiveLogicServiceEnabled { get; private set; }
        public ILog Logger { get; private set; }
    }

    public class Launcher : Obj
    {
        public static Launcher Ins { get; private set; }

        /// <summary>
        ///     逻辑服务
        /// </summary>
        public ILogicService LogicService
        {
            get { return GlobalVarPool.Ins.LogicService; }
        }

        /// <summary>
        ///     网络服务
        /// </summary>
        public INetService NetService
        {
            get { return GlobalVarPool.Ins.NetService; }
        }

        protected override void OnInit(ObjArg arg)
        {
            base.OnInit(arg);

            if(Ins != null)
                throw new Exception("Launcher already instantiated!");
            Ins = this;

            var more = arg.As<LauncherArg>();

            // logger
            GlobalVarPool.Ins.Set(GlobalVarPool.NameOfLogger, more.Logger ?? new DefaultLogger());

            // logic service
            var serviceArg = new ServiceArg(this, 10000, 10);
            var logicService = more.PassiveLogicServiceEnabled
                ? Create<PassiveLogicService>(serviceArg)
                : (ILogicService)Create<AutoLogicService>(serviceArg);
            GlobalVarPool.Ins.Set(GlobalVarPool.NameOfLogicService, logicService);

            // net service
            var netService = Create<NetService>(serviceArg);
            GlobalVarPool.Ins.Set(GlobalVarPool.NameOfNetService, netService);
        }

        protected override void OnStart()
        {
            base.OnStart();
            NetService.Start();
            LogicService.Start();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            NetService.Destroy();
            LogicService.Destroy();
            Logger.Ins.Shutdown();
        }
    }
}
