using System.Configuration;
using System.Text;

namespace node
{
    public class RedisElement : ConfigElement
    {
        [ConfigurationProperty("category",
            IsRequired = true)]
        public string Category
        {
            get { return (string)this["category"]; }
        }

        [ConfigurationProperty("distributeid",
              IsRequired = false)]
        public int DistributeId
        {
            get { return (int)this["distributeid"]; }
        }

        [ConfigurationProperty("ip",
              IsRequired = true)]
        public string Ip
        {
            get { return (string)this["ip"]; }
        }

        [ConfigurationProperty("port",
            IsRequired = true)]
        public ushort Port
        {
            get { return (ushort)this["port"]; }
            set { this["port"] = value; }
        }

        [ConfigurationProperty("pwd",
            DefaultValue = "",
            IsRequired = false)]
        public string Pwd
        {
            get { return (string)this["pwd"]; }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine(base.ToString());
            sb.AppendFormat("Category : {0}\r\n", Category);
            sb.AppendFormat("Ip : {0}\r\n", Ip);
            sb.AppendFormat("Port : {0}\r\n", Port);
            sb.AppendFormat("Pwd : {0}\r\n", Pwd);
            return sb.ToString();
        }
    }
}
