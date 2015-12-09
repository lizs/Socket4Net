namespace socket4net
{
    public static class ObjFactory
    {
        public static T Create<T>(ObjArg arg) where T : IObj, new()
        {
            var ret = new T();
            ret.SetArgument(arg);
            return ret;
        }

        public static T Create<T>() where T : IObj, new()
        {
            var ret = new T();
            ret.SetArgument(null);
            return ret;
        }
    }
}
