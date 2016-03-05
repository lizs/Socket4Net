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
