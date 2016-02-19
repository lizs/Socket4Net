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
        public static Launcher Instance { get; private set; }

        /// <summary>
        ///     逻辑服务
        /// </summary>
        public ILogicService LogicService
        {
            get { return GlobalVarPool.Instance.LogicService; }
        }

        /// <summary>
        ///     网络服务
        /// </summary>
        public INetService NetService
        {
            get { return GlobalVarPool.Instance.NetService; }
        }

        protected override void OnInit(ObjArg arg)
        {
            base.OnInit(arg);

            if(Instance != null)
                throw new Exception("Launcher already instantiated!");
            Instance = this;

            var more = arg.As<LauncherArg>();

            // logger
            GlobalVarPool.Instance.Set(GlobalVarPool.NameOfLogger, more.Logger ?? new DefaultLogger());

            // logic service
            var serviceArg = new ServiceArg(this, 10000, 10);
            var logicService = more.PassiveLogicServiceEnabled
                ? Create<PassiveLogicService>(serviceArg)
                : (ILogicService)Create<AutoLogicService>(serviceArg);
            GlobalVarPool.Instance.Set(GlobalVarPool.NameOfLogicService, logicService);

            // net service
            var netService = Create<NetService>(serviceArg);
            GlobalVarPool.Instance.Set(GlobalVarPool.NameOfNetService, netService);

            // sys
            Create<ScheduleSys>(new ObjArg(this));
        }

        protected override void OnStart()
        {
            base.OnStart();
            NetService.Start();
            LogicService.Start();
            ScheduleSys.Instance.Start();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            NetService.Perform(()=>NetService.Destroy());
            LogicService.Perform(() =>
            {
                ScheduleSys.Instance.Destroy();
                LogicService.Destroy();

                Logger.Instance.Shutdown();
            });
        }
    }
}
