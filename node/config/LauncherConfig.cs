using System.Configuration;
using System.Text;

namespace node
{
    public class LauncherConfig : ConfigurationSection
    {
        [ConfigurationProperty("logconfig")]
        public LogConfigElement LogConfig
        {
            get { return this["logconfig"] as LogConfigElement; }
        }

        [ConfigurationProperty("servers")]
        public ServerCollection Servers
        {
            get { return this["servers"] as ServerCollection; }
        }

        [ConfigurationProperty("clients")]
        public ClientCollection Clients
        {
            get { return this["clients"] as ClientCollection; }
        }

        [ConfigurationProperty("redisnodes")]
        public RedisElementCollection RedisNodes
        {
            get { return this["redisnodes"] as RedisElementCollection; }
        }


        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(LogConfig);
            sb.Append(RedisNodes);
            sb.Append(Servers);
            return sb.ToString();
        }

        /// <summary>
        ///     º”‘ÿApp.Config
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="nodeCfgName"></param>
        /// <returns></returns>
        public static T LoadAs<T>(string path, string nodeCfgName = "LauncherConfig") where T : class
        {
            var map = new ExeConfigurationFileMap { ExeConfigFilename = path };
            var config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
            return config.GetSection(nodeCfgName) as T;
        }
    }
}