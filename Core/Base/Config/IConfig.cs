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
