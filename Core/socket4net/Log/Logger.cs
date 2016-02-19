namespace socket4net
{
    public static class Logger
    {
        private static ILog _ins;
        public static ILog Ins
        {
            get { return _ins ?? (_ins = GlobalVarPool.Ins.Logger); }
        }
    }
}
