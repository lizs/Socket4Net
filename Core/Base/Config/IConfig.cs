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
    /// csv 解析器
    /// </summary>
    public interface IParser
    {
        object Config { get; }
        void Parse(string path);
    }

    /// <summary>
    ///     结构化解析器
    /// </summary>
    public interface IStructParser
    {
        object Config { get; }
        void Parse(string path);
    }

    /// <summary>
    /// k-v 解析器
    /// </summary>
    public interface IKeyValueParser
    {
        void Parse(IKeyValue container, string path);
    }

    /// <summary>
    ///     csv配置
    /// </summary>
    public interface ICsv
    {
        int Id { get; }
    }

    /// <summary>
    ///     结构化配置
    /// </summary>
    public interface IStruct
    {
    }

    /// <summary>
    /// key-value 配置
    /// </summary>
    public interface IKeyValue
    {
        string Get(string key);
        bool Has(string key);
        void Add(string key, string value);
    }

    /// <summary>
    /// 其成员拥有OrderAttribute的配置
    /// </summary>
    public interface IOrdered
    {
    }

    /// <summary>
    /// 两层csv配置
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRichCsv<T> : ICsv where T : ICsv
    {
        Dictionary<int, T> Children { get; }
    }

    /// <summary>
    /// 实体配置
    /// </summary>
    public interface IEntityCsv : ICsv
    {
        string Name { get; }
        string BaseModel { get; }
    }
}
