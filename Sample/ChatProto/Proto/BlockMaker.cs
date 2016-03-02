using System;
using socket4net;

namespace Shared
{
    public static class BlockMaker
    {
        public static IBlock Create(short pid)
        {
            return Create((EPid) pid);
        }

        public static IBlock Create(EPid epid)
        {
            if (!Enum.IsDefined(typeof(EPid), epid))
                throw new ArgumentException("epid");

            var pid = (short) epid;
            switch (epid)
            {
                case EPid.One:
                    return new SettableBlock<int>(pid, 1, EBlockMode.Synchronizable);

                case EPid.Two:
                    return new IncreasableBlock<int>(pid, 2, EBlockMode.Synchronizable, 0, 10);

                case EPid.Three:
                    return new ListBlock<int>(pid, new []{1, 2, 3}, EBlockMode.Synchronizable);

                default:
                    throw new NotImplementedException(pid.ToString());
            }
        }
    }
}
