using System.Configuration;
using System.Text;
using node;

namespace Sample
{
    public class ChatElement : ServerNodeElement
    {
        [ConfigurationProperty("redisnodes")]
        public RedisElementCollection RedisNodes
        {
            get { return this["redisnodes"] as RedisElementCollection; }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine(base.ToString());
            sb.AppendLine(RedisNodes.ToString());
            return sb.ToString();
        }
    }
}
