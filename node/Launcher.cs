using System;
using socket4net;

namespace node
{
    public class LauncherArg<T> : LauncherArg where T : LauncherConfig
    {
        public T Config { get; private set; }

        public LauncherArg(T cfg, ILog logger, bool passiveLogicServiceEnabled = false)
            : base(logger, null, passiveLogicServiceEnabled)
        {
            Config = cfg;
        }
    }

    public class Launcher<T> : Launcher where T : LauncherConfig
    {
        protected readonly Mgr<Obj> Jobs = New<Mgr<Obj>>(ObjArg.Empty, false);
        public T Config { get; private set; }
        public Guid Id { get; private set; }

        protected override void OnInit(ObjArg arg)
        {
            base.OnInit(arg);
            var more = arg.As<LauncherArg<T>>();
            Config = more.Config;
            Id = more.Id;

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
