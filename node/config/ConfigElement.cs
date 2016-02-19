using System.Configuration;

namespace node
{
    public abstract class ConfigElement : ConfigurationElement
    {
        [ConfigurationProperty("name",
          DefaultValue = "未命名",
          IsRequired = true)]
        public string Name
        {
            get
            {
                return (string)this["name"];
            }
        }

        public override string ToString()
        {
            return string.Format("Name : {0}", Name);
        }
    }
}