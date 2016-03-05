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
        private static GlobalVarPool _ins;
        public static GlobalVarPool Ins { get { return _ins ?? (_ins = new GlobalVarPool()); } }

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
