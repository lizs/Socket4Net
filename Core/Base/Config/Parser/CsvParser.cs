using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace socket4net
{
    public class OrderComparer : IComparer<PropertyInfo>
    {
        public int Compare(PropertyInfo x, PropertyInfo y)
        {
            var orderx = GetOrder(x);
            var ordery = GetOrder(y);
            if (orderx == ordery) return 0;
            if (orderx < ordery) return -1;
            return 1;
        }

        private int GetOrder(PropertyInfo propInfo)
        {
            var orderAttr = (OrderAttribute)propInfo.GetCustomAttributes(typeof(OrderAttribute), true).SingleOrDefault();
            return orderAttr != null ? orderAttr.Order : Int32.MaxValue;
        }
    }

    public sealed class ConfigOrder<T>
    {
        public static int GetIndex(PropertyInfo pInfo)
        {
            var attrs = pInfo.GetCustomAttributes(false);
            foreach (var attr in attrs.Where(attr => attr.GetType() == typeof(OrderAttribute)))
            {
                return (attr as OrderAttribute).Order;
            }
            return -1;
        }

        public static object GetObejct(T o, int index)
        {
            var properties = o.GetType().GetProperties();
            return (from pInfo in properties let i = GetIndex(pInfo) where index == i select pInfo.GetValue(o, null)).FirstOrDefault();
        }
    }

    public class Parser<T> : IParser where T : ICsv
    {
        private readonly IFileLoader _loader;
        public Parser(IFileLoader loader)
        {
            _loader = loader;
        }

        public void Parse(string path)
        {
            var config = new Dictionary<int, T>();

            var text = _loader.Read(path);
            if (string.IsNullOrEmpty(text))
            {
                Logger.Instance.FatalFormat("Parse {0} failed!", path);
                return;
            }

            var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            var properties = typeof(IOrdered).IsAssignableFrom(typeof(T)) ?
                typeof(T).GetProperties().OrderBy(p=>p, new OrderComparer()).ToArray()
                : typeof (T).GetProperties();

            var lineNum = 0; // 行数，方便出错时查找
            foreach (var line in lines)
            {
                lineNum += 1;

                // # ... => comment
                if (line.TrimStart(' ')[0] == '#')
                    continue;

                var words = line.Split(new[] { ',' });
                if (words.Length != properties.Length)
                {
                    throw new Exception(string.Format("Type '{0}' and config file '{1}' mismatch!!!", typeof(T), path));
                }

                var item = (T)Activator.CreateInstance(typeof(T));
                for (var i = 0; i < words.Length; ++i)
                {
                    // 去掉空格
                    words[i] = words[i].Trim();

                    try
                    {
                        var value = StringParser.ParseValue(properties[i], words[i]);
                        properties[i].SetValue(item, value, null);
                    }
                    catch (Exception e)
                    {
                        var msg = string.Format("ParseValue line[{0}] property[{1}] word[{2}] failed, text[{3}], error[{4}]",
                            lineNum, properties[i], words[i], line, e.Message);

                        throw new Exception(msg);
                    }
                }

                var id = (int) properties[0].GetValue(item, null);
                if(!config.ContainsKey(id))
                    config.Add(id, item);
                else
                    Logger.Instance.FatalFormat("Key {0} of {1} already exists!", id, path);
            }

            Config = config;
        }

        public object Config { get; private set; }
    }
}