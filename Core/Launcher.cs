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

namespace socket4net
{
    public class LauncherArg : ObjArg
    {
        public LauncherArg(ILog logger = null, Guid? id = null, bool passiveLogicServiceEnabled = false)
            : base(null)
        {
            Logger = logger;
            PassiveLogicServiceEnabled = passiveLogicServiceEnabled;
            Id = id ?? new Guid();
        }

        public bool PassiveLogicServiceEnabled { get; private set; }
        public ILog Logger { get; private set; }
        public Guid Id { get; private set; }
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

            if (Ins != null)
                throw new Exception("Launcher already instantiated!");
            Ins = this;

            var more = arg.As<LauncherArg>();

            // logger
            GlobalVarPool.Ins.Set(GlobalVarPool.NameOfLogger, more.Logger);

            // logic service
            var serviceArg = new ServiceArg(this, 10000, 10);
            var logicService = more.PassiveLogicServiceEnabled
                ? New<PassiveLogicService>(serviceArg)
                : (ILogicService)New<AutoLogicService>(serviceArg);
            GlobalVarPool.Ins.Set(GlobalVarPool.NameOfLogicService, logicService);

            // net service
            var netService = New<NetService>(serviceArg);
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

            Ins = null;
        }
    }
}
