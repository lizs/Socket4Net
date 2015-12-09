using System.Collections.Generic;

namespace socket4net
{
    /// <summary>
    ///     全局变量池
    /// </summary>
    public class GlobalVarPool
    {
        public const string NameOfLogicService = "LogicService";
        public const string NameOfNetService = "NetService";
        public const string NameOfLogger = "Logger";
        
        private GlobalVarPool(){}
        private static GlobalVarPool _instance;
        public static GlobalVarPool Instance { get { return _instance ?? (_instance = new GlobalVarPool()); } }

        private readonly Dictionary<string, object> _vars = new Dictionary<string, object>();
        public T Get<T>(string key)
        {
            if (_vars.ContainsKey(key))
                return (T)_vars[key];

            return default(T);
        }

        public void Set<T>(string key, T var)
        {
            _vars[key] = var;
            if (key == NameOfLogicService)
                _logicService = null;
            if (key == NameOfNetService)
                _netService = null;
        }

        private ILogicService _logicService;
        public ILogicService LogicService
        {
            get { return _logicService ?? (_logicService = Get<ILogicService>(NameOfLogicService)); }
        }

        private INetService _netService;
        public INetService NetService
        {
            get { return _netService ?? (_netService = Get<INetService>(NameOfNetService)); }
        }

        private ILog _logger;
        public ILog Logger
        {
            get { return _logger ?? (_logger = Get<ILog>(NameOfLogger)); }
        }
    }
}
