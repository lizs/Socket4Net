using System.Configuration;
using System.Text;

namespace node
{
    public class NodeCollection<T> : ConfigElementCollection<T> where T : ConfigElement, new()
    {
        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var node in this)
            {
                sb.AppendLine("---------------------");
                sb.AppendLine(node.ToString());
            }

            return sb.ToString();
        }
    }


    [ConfigurationCollection(typeof(ServerNodeElement), AddItemName = "node")]
    public class ServerCollection : NodeCollection<ServerNodeElement>
    {
    }

    [ConfigurationCollection(typeof(ClientNodeElement), AddItemName = "node")]
    public class ClientCollection : NodeCollection<ClientNodeElement>
    {
    }

    [ConfigurationCollection(typeof(NodeElement), AddItemName = "node")]
    public class RedisElementCollection : NodeCollection<NodeElement>
    {
    }
}