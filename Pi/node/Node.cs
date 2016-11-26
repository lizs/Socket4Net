using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using socket4net;

namespace Pi
{
    internal enum ENodeType
    {
        Server,
        Client
    }

    public class ClientConfig
    {
        public Type TypeOfClient { get; set; }
        public string FullUrl { get; set; }
    }

    /// <summary>
    ///     server config 
    /// </summary>
    public class ServerConfig
    {
        /// <summary>
        ///     server root url
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        ///     servers config
        /// </summary>
        public List<Pair<Type, string>> Servers { get; set; }
    }

    /// <summary>
    ///     arguments to initalize a node
    /// </summary>
    public class NodeArg : UniqueObjArg<short>
    {
        /// <summary>
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="key"></param>
        public NodeArg(IObj owner, short key) : base(owner, key)
        {
        }

        /// <summary>
        ///     servers
        /// </summary>
        public ServerConfig ServerConfig { get; set; }

        /// <summary>
        ///     clients
        /// </summary>
        public ClientConfig ClientConfig { get; set; }

        /// <summary>
        ///     convert to json string
        /// </summary>
        /// <returns></returns>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        /// <summary>
        ///     save as json file
        /// </summary>
        /// <returns></returns>
        public void ToJsonFile(string path)
        {
            var json = ToJson();
            FileSys.WriteToFile(path, json);
        }

        /// <summary>
        ///     parse from json string
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static NodeArg FromJsonString(string json)
        {
            return JsonConvert.DeserializeObject<NodeArg>(json);
        }

        /// <summary>
        ///     parse from json file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static NodeArg FromJsonFile(string path)
        {
            var json = FileSys.ReadFile(path);
            return json.IsNullOrWhiteSpace() ? null : FromJsonString(json);
        }
    }

    /// <summary>
    ///     node
    ///     abstraction of client/fronted sever/backend server
    /// </summary>
    public class Node : UniqueObj<short>
    {
        /// <summary>
        ///    internal called when an Obj is initialized
        /// </summary>
        /// <param name="arg"></param>
        protected override void OnInit(ObjArg arg)
        {
            base.OnInit(arg);

            var more = arg as NodeArg;

            // create servers

            // create clients
            var clientsConfig = more?.ClientConfig;
        }

        protected override void OnStart()
        {
            base.OnStart();
        }
    }
}
