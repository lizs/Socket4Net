using System;
using System.Linq;
using System.Reflection;

namespace socket4net
{
    /// <summary>
    ///     结构化解析器
    ///     解析配置到一个单一的类对象中
    /// </summary>
    public class StructParser<T> : IStructParser where T : class
    {
        public T Result { get; private set; }
        public object Config
        {
            get { return Result; }
        }

        private readonly IFileLoader _loader;
        public StructParser(IFileLoader loader)
        {
            _loader = loader;
        }
        
        private void ParseLine(PropertyInfo propertyInfo, string line)
        {
            var value = StringParser.ParseValue(propertyInfo, line);
            propertyInfo.SetValue(Result, value, null);
        }

        public void Parse(string path)
        {
            var text = _loader.Read(path);
            if (string.IsNullOrEmpty(text))
            {
                throw new Exception(string.Format("Parse {0} failed!", path));
            }

            var lines = text.Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);

            var properties = typeof(IOrdered).IsAssignableFrom(typeof(T)) ?
                typeof(T).GetProperties().OrderBy(p=>p, new OrderComparer()).ToArray()
                : typeof (T).GetProperties();

            var filteredLines = lines.Where(x => x.TrimStart(' ')[0] != '#').ToList();

            if (filteredLines.Count != properties.Length)
                throw new Exception(string.Format("Type '{0}' and config file '{1}' mismatch!!!", typeof(T), path));

            Result = (T)Activator.CreateInstance(typeof(T));
            for (var i = 0; i < properties.Length; ++i)
            {
                ParseLine(properties[i], filteredLines[i]);
            }
        }
    }
}
