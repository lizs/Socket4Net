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

namespace socket4net
{
    public class KeyValueParser : IKeyValueParser
    {
        private readonly IFileLoader _loader;
        public KeyValueParser(IFileLoader loader)
        {
            _loader = loader;
        } 

        public void Parse(IKeyValue container, string path)
        {
            var text = _loader.Read(path);
            if (string.IsNullOrEmpty(text)) 
            {
                Logger.Ins.Fatal("Parse {0} failed!", path);
                return;
            }

            var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                // # ... => comment
                if (line.TrimStart(' ')[0] == '#')
                    continue;

                var words = line.Split(new [] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (words.Length != 2)
                {
                    throw new Exception("Invalid key-value configuration file : " + path);
                }
                
               container.Add(words[0], words[1]);
            }
        }
    }
}