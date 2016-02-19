using System.Collections.Generic;
using System.Configuration;

namespace node
{
    /// <summary>
    ///     配置节点集合
    /// </summary>
    /// <typeparam name="TElement"></typeparam>
    public class ConfigElementCollection<TElement> : ConfigurationElementCollection, IEnumerable<TElement>
        where TElement : ConfigElement, new()
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new TElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return element;
        }

        public new IEnumerator<TElement> GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
            {
                yield return (TElement)BaseGet(i);
            }
        }
    }
}
