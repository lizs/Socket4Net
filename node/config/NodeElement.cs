using System;
using System.Configuration;
using System.Text;

namespace node
{
    public class ClientNodeElement : NodeElement
    {
        [ConfigurationProperty("pwd",
               IsRequired = true)]
        public string Pwd
        {
            get { return (string)this["pwd"]; }
        }

        [ConfigurationProperty("autoreconnect",
            DefaultValue = true,
            IsRequired = true)]
        public bool AutoReconnect
        {
            get { return (bool)this["autoreconnect"]; }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine(base.ToString());
            sb.AppendLine("AutoReconnect : " + AutoReconnect);
            sb.AppendLine("Pwd : " + Pwd);
            return sb.ToString();
        }
    }

    public class ServerNodeElement : NodeElement
    {
        [ConfigurationProperty("pwd",
               IsRequired = true)]
        public string Pwd
        {
            get { return (string)this["pwd"]; }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine(base.ToString());
            sb.AppendLine("Pwd : " + Pwd);
            return sb.ToString();
        }
    }

    public class NodeElement : ConfigElement
    {
        /// <summary>
        ///     唯一Id
        /// </summary>
        [ConfigurationProperty("guid",
            IsRequired = true)]
        public Guid Guid
        {
            get { return (Guid)this["guid"]; }
        }

        /// <summary>
        ///     节点类型
        /// </summary>
        [ConfigurationProperty("category",
            IsRequired = true)]
        public string Category
        {
            get { return (string)this["category"]; }
        }

        /// <summary>
        ///     监控
        /// </summary>
        [ConfigurationProperty("monitorenabled",
            DefaultValue = false,
            IsRequired = false)]
        public bool MonitorEnabled
        {
            get { return (bool)this["monitorenabled"]; }
        }

        /// <summary>
        ///     ip
        /// </summary>
        [ConfigurationProperty("ip",
            IsRequired = true)]
        public string Ip
        {
            get { return (string) this["ip"]; }
        }

        /// <summary>
        ///     端口
        /// </summary>
        [ConfigurationProperty("port",
            IsRequired = true)]
        public ushort Port
        {
            get { return (ushort) this["port"]; }
            set { this["port"] = value; }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine(base.ToString());
            sb.AppendFormat("Guid : {0}\r\n", Guid);
            sb.AppendFormat("Category : {0}\r\n", Category);
            //sb.AppendFormat("File : {0}\r\n", LogConfig);
            sb.AppendFormat("MonitorEnabled : {0}\r\n", MonitorEnabled);
            sb.AppendFormat("Ip : {0}\r\n", Ip);
            sb.AppendFormat("Port : {0}", Port);

            return sb.ToString();
        }
    }
}