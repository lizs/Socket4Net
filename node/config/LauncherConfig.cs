#region MIT
//  /*The MIT License (MIT)
// 
//  Copyright 2016 lizs lizs4ever@163.com
//  
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//  
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
//  
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
//   * */
#endregion
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