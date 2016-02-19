using System.Configuration;
using System.Text;

namespace node
{
    public class LogConfigElement : ConfigElement
    {
        [ConfigurationProperty("file", IsRequired = true, DefaultValue = "log4net.config")]
        public string File
        {
            get { return (string)this["file"]; }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine(base.ToString());
            sb.AppendLine(string.Format("File : {0}", File));
            return sb.ToString();
        }
    }
}