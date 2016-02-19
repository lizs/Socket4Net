using System;
using socket4net;

namespace node
{
    public class LauncherArg<T> : LauncherArg where T : IServerConfig
    {
        public T Config { get; private set; }

        public LauncherArg(T cfg, ILog logger, bool passiveLogicServiceEnabled = false)
            : base(logger, passiveLogicServiceEnabled)
        {
            Config = cfg;
        }
    }

    public class Launcher<T> : Launcher where T : IServerConfig
    {
        protected readonly Mgr<Obj> Jobs = Create<Mgr<Obj>>(ObjArg.Empty);
        public T Config { get; private set; }

        protected override void OnInit(ObjArg arg)
        {
            base.OnInit(arg);
            var more = arg.As<LauncherArg<T>>();
            Config = more.Config;

            SpawnJobs();
        }

        protected override void OnDestroy()
        {
            GlobalVarPool.Ins.LogicService.Perform(() => Jobs.Destroy());
            base.OnDestroy();
        }

        protected override void OnStart()
        {
            base.OnStart();
            GlobalVarPool.Ins.LogicService.Perform(() => Jobs.Start());
        }

        protected virtual void SpawnJobs()
        {
        }
    }
}
