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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace socket4net
{
    // 以首列是否为空判断层级
    public class RichParser<T> : IParser where T : ICsv
    {
        private readonly IFileLoader _loader;

        public RichParser(IFileLoader loader)
        {
            _loader = loader;
        }

        private class ItemInfo
        {
            public Type Type;
            public IDictionary Dic;
        }

        public object Config { get; private set; }

        // 字典栈
        private readonly Stack<ItemInfo> _dictinaryStack = new Stack<ItemInfo>();

        // 解析当前行
        private void ParseLine(string line, int lineNum)
        {
            // # ... => 注释
            if (line.TrimStart(' ')[0] == '#')
                return;

            var _words = line.Split(new[] {','});
            if (_words[0].Length != 0 && _dictinaryStack.Count > 1)
            {
                // 层级减少，退回上一层处理
                _dictinaryStack.Pop();
            }

            // 干掉空字，直到非空字出现为止
            var words = new List<string>();
            var useful = false;
            foreach (var word in _words)
            {
                if (word.Length != 0)
                    useful = true;

                if (useful)
                    words.Add(word);
            }

            PropertyInfo[] properties = null;
            var top = _dictinaryStack.Peek();

            properties = typeof (IOrdered).IsAssignableFrom(top.Type)
                ? top.Type.GetProperties().OrderBy(p => p, new OrderComparer()).ToArray()
                : top.Type.GetProperties();

            // 创建行对象
            var constructorInfo = top.Type.GetConstructor(Type.EmptyTypes);
            var item = constructorInfo.Invoke(null);
            for (var i = 0; i < properties.Length; ++i)
            {
                if (typeof (IDictionary).IsAssignableFrom(properties[i].PropertyType))
                {
                    var args = properties[i].PropertyType.GetGenericArguments();

                    // 将该字典入栈
                    PushDictionaryStack(args[1]);

                    // 赋值
                    properties[i].SetValue(item, _dictinaryStack.Peek().Dic, null);

                    // 字典必须为尾成员
                    break;
                }

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

            // 增加记录
            top.Dic.Add((item as ICsv).Id, item);
        }

        private void PushDictionaryStack(Type mebType)
        {
            var dic = CreateDic(mebType);

            var item = new ItemInfo {Dic = dic as IDictionary, Type = mebType};

            _dictinaryStack.Push(item);
        }

        private object CreateDic(Type mebType)
        {
            var unkonwn = typeof (Dictionary<,>);
            Type[] typeArgs = {typeof (int), mebType};
            var generic = unkonwn.MakeGenericType(typeArgs);

            var constructor = generic.GetConstructor(Type.EmptyTypes);
            return constructor.Invoke(null);
        }

        public void Parse(string path)
        {
            var text = _loader.Read(path);
            if (string.IsNullOrEmpty(text))
            {
                Logger.Ins.Fatal("Parse {0} failed!", path);
                return;
            }

            var lines = text.Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);

            // 根字典入栈
            PushDictionaryStack(typeof (T));

            var lineNum = 0; // 行数，方便出错时查找
            foreach (var line in lines)
            {
                lineNum += 1;
                ParseLine(line, lineNum);
            }

            _dictinaryStack.Pop();
            if (_dictinaryStack.Count > 0)
            {
                Config = _dictinaryStack.Peek().Dic as Dictionary<int, T>;   
            }
            else
            {
                Config = new Dictionary<int, T>();
            }
        }
    }
}