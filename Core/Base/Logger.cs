namespace socket4net
{
    public static class Logger
    {
        private static ILog _instance;
        public static ILog Instance
        {
            get { return _instance ?? (_instance = GlobalVarPool.Instance.Logger); }
        }
    }
}
