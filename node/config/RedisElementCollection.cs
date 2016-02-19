using System.Configuration;
using System.Text;

namespace node
{
    [ConfigurationCollection(typeof(RedisElement), AddItemName = "node")]
    public class RedisElementCollection : ConfigElementCollection<RedisElement>
    {
        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var redisNode in this)
            {
                sb.AppendLine("---------------------");
                sb.AppendLine(redisNode.ToString());
            }

            return sb.ToString();
        }
    }
}
