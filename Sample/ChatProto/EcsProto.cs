using System;
using socket4net;

namespace Proto
{
    public enum EPid
    {
        One,
        Two,
        Three,
    }

    public static class BlockMaker
    {
        public static IBlock Create(short pid)
        {
            if(!Enum.IsDefined(typeof(EPid), pid))
                throw new ArgumentException("pid");

            switch ((EPid)pid)
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
