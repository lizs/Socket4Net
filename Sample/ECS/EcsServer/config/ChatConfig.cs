using System.Configuration;
using System.Text;
using node;

namespace Sample
{
    public class ChatConfig : ConfigurationSection, IServerConfig
    {
        [ConfigurationProperty("logconfig")]
        public LogConfigElement LogConfig
        {
            get { return this["logconfig"] as LogConfigElement; }
        }

        [ConfigurationProperty("chat")]
        public ChatElement Chat
        {
            get { return this["chat"] as ChatElement; }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("=====================");
            sb.AppendLine(LogConfig.ToString());
            sb.AppendLine("---------------------");
            sb.AppendLine(Chat.ToString());
            sb.AppendLine("=====================");
            return sb.ToString();
        }
    }
}
