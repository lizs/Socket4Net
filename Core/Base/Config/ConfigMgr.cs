using System;
using System.Collections;
using System.Collections.Generic;

namespace socket4net
{
    public abstract class KeyValue : IKeyValue
    {
        private readonly Dictionary<string, string> _dic = new Dictionary<string, string>();

        public void Add(string key, string value)
        {
            _dic.Add(key, value);
        }

        public string Get(string key)
        {
            return _dic.ContainsKey(key) ? _dic[key] : string.Empty;
        }

        public bool Has(string key)
        {
            return _dic.ContainsKey(key);
        }
    }

    public class ConfigMgrArg : ObjArg
    {
        public IFileLoader FileLoader { get; private set; }
        public string NameSpace { get; private set; }
        public ConfigMgrArg(IObj owner, IFileLoader loader, string ns = null) : base(owner)
        {
            FileLoader = loader;
            NameSpace = ns;
        }
    }

    public class ConfigMgr : StrictTask
    {
        // for IBatch<string>
        public ConfigMgr()
        {
            if (Instance != null)
            {
                throw new Exception("ConfigMgr already Instantiated!");
            }
            Instance = this;
        }

        public static ConfigMgr Instance { get; private set; }

        // 存放配置文件相对路径
        readonly List<string> _summary = new List<string>();

        // 存放csv配置
        readonly Dictionary<string, object> _csvConfigs = new Dictionary<string, object>();

        // 存放key-value配置
        readonly Dictionary<string, IKeyValue> _kvConfigs = new Dictionary<string, IKeyValue>();

        // 存放struct配置
        readonly Dictionary<string, object> _structConfigs = new Dictionary<string, object>();

        private IFileLoader _loader;
        private string _configNamespace = "Pi.BusinessShared";
        private string _configAssemblyName = "Pi.BusinessShared";

        public string ConfigNamespace
        {
            get { return _configNamespace; }
            set { _configNamespace = value; }
        }

        public string ConfigAssemblyName
        {
            get { return _configAssemblyName; }
            set { _configAssemblyName = value; }
        }

        /// <summary>
        ///     执行初始化
        /// </summary>
        /// <param name="arg"></param>
        protected override void OnInit(ObjArg objArg)
        {
            base.OnInit(objArg);

            var more = objArg.As<ConfigMgrArg>();
            if ( more.FileLoader == null) throw new ArgumentException();

            _loader = more.FileLoader;
            if (!string.IsNullOrEmpty(more.NameSpace))
                _configNamespace = more.NameSpace;

            var parser = new Parser<Summary>(_loader);
            parser.Parse("Config/Summary");

            var summaryCfg = parser.Config as Dictionary<int, Summary>;
            foreach (var s in summaryCfg)
            {
                _summary.Add(s.Value.Path);
            }

            Steps = _summary.Count;
        }
        
        // 根据Id获取csv配置
        public T GetCsv<T>(int id) where T : class, ICsv
        {
            var cfg = GetCsv<T>();
            return cfg.ContainsKey(id) ? cfg[id] : default(T);
        }

        // 根据类型获取csv配置
        public Dictionary<int, T> GetCsv<T>() where T : ICsv
        {
            var cfg = GetCsv(typeof (T));
            return cfg as Dictionary<int, T>;
        }

        // 根据类型获取csv配置
        public object GetCsv(Type type)
        {
            var key = type.FullName;
            return _csvConfigs.ContainsKey(key) ? _csvConfigs[key] : null;
        }

        /// <summary>
        /// 获取Key-Vaue配置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetKeyValue<T>() where T : class, IKeyValue
        {
            var key = typeof (T).FullName;
            return _kvConfigs.ContainsKey(key) ? _kvConfigs[key] as T: null;
        }

        /// <summary>
        ///     获取value
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <typeparam name="TClass"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetValue<TClass>(string key) where TClass : KeyValue
        {
            var cfg = GetKeyValue<TClass>();
            return cfg != null ? cfg.Get(key) : string.Empty;
        }

        /// <summary>
        ///     获取结构化配置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetStruct<T>()
        {
            var key = typeof(T).FullName;
            return (T)(_structConfigs.ContainsKey(key) ? _structConfigs[key] : null);
        }
        
        public TU GetCsv<T, TU>(int i, int j)
            where T : IRichCsv<TU>, new()
            where TU : class, ICsv
        {
            var cfg = GetCsv<T>();
            if (!cfg.ContainsKey(i)) return null;
            var t = cfg[i];

            return t.Children.ContainsKey(j) ? t.Children[j] : null;
        }

        private void ParseStruct(Type gen, string typeName, string path)
        {
            string fullName;
            string fullNameWioutAssembly;
            var type = GetType(typeName, out fullName, out fullNameWioutAssembly);

            var generic = gen.MakeGenericType(new[] { type });
            var constructor = generic.GetConstructor(new[] { typeof(IFileLoader) });
            var parser = constructor.Invoke(new object[] { _loader }) as IStructParser;
            parser.Parse(path);

            _structConfigs[fullNameWioutAssembly] = parser.Config;
        }

        private void ParseKeyValue(string typeName, string path)
        {
            string fullName;
            string fullNameWioutAssembly;
            var type = GetType(typeName, out fullName, out fullNameWioutAssembly);

            var config = Activator.CreateInstance(type) as IKeyValue;
            var parser = new KeyValueParser(_loader);
            parser.Parse(config, path);

            _kvConfigs[fullNameWioutAssembly] = config;
        }

        void ParseCsv(Type gen, string typeName, string path)
        {
            string fullName;
            string fullNameWioutAssembly;
            var type = GetType(typeName, out fullName, out fullNameWioutAssembly);

            var generic = gen.MakeGenericType(new[] { type });
            var constructor = generic.GetConstructor(new[] { typeof(IFileLoader) });
            var parser = constructor.Invoke(new object[] { _loader }) as IParser;
            parser.Parse(path);

            _csvConfigs[fullNameWioutAssembly] = parser.Config;
        }

        private Type GetType(string typeName, out string fullName, out string fullNameWithoutAssembly)
        {
            fullNameWithoutAssembly = string.Concat(ConfigNamespace, ".", typeName);
            fullName = string.Concat(fullNameWithoutAssembly, ",", ConfigAssemblyName);

            var type = Type.GetType(fullName);
            if (type == null)
            {
                Logger.Instance.FatalFormat("Type of config : {0} doesn't exist!!!", fullName);
                throw new Exception(string.Format("Type of config : {0} doesn't exist!!!", fullName));
            }
            return type;
        }

        // for task
        public override IEnumerator Step()
        {
            foreach (var path in _summary)
            {
                LoadOne(path);

                yield return null;
            }

            IsDone = true;
        }

        public void LoadAll()
        {
            foreach (var path in _summary)
                LoadOne(path);

            // todo lizs
            //GoodsConvertor.Instance.Cache();

            IsDone = true;
        }

        private void LoadOne(string path)
        {
            Logger.Instance.Info("Parse : " + path);
            var info = path.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
            switch (info[1])
            {
                case "Csv":
                    ParseCsv(typeof (Parser), info[2], path);
                    break;

                case "RichCsv":
                    ParseCsv(typeof (RichParser), info[2], path);
                    break;

                case "KeyValue":
                    ParseKeyValue(info[2], path);
                    break;

                case "Struct":
                    ParseStruct(typeof(StructParser), info[2], path);
                    break;

                default:
                    throw new Exception(string.Format("Unknown type : {0}", info[1]));
            }
            Logger.Instance.Info("Parse : " + path + " success!!!");
        }
    }
}
