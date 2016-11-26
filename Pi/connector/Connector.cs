using System;
using System.Collections.Generic;
using socket4net;
using WebSocketSharp.Server;

namespace Pi
{
    /// <summary>
    ///     connector arguments
    /// </summary>
    public class ConnectorArg : UniqueObjArg<Guid>
    {
        /// <summary>
        ///     urlForClients
        /// </summary>
        public string UrlForClients { get; }

        /// <summary>
        ///     urls of exchangers
        /// </summary>
        public string[] UrlOfExchangers { get; }

        /// <summary>
        ///     ctor
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="key"></param>
        /// <param name="urlForClients"></param>
        /// <param name="urlOfExchangers"></param>
        public ConnectorArg(IObj owner, Guid key, string urlForClients, string[] urlOfExchangers) : base(owner, key)
        {
            UrlForClients = urlForClients;
            UrlOfExchangers = urlOfExchangers;
        }
    }

    /// <summary>
    ///     connector
    ///     1 maintain clients sessions
    ///     2 foward clients request/push to exchangers
    ///     2 response/push exchangers message to clients
    /// </summary>
    public class Connector<TClientSession, TExchangerClient> : UniqueObj<short>
        where TClientSession : WebsocketSession, new()
        where TExchangerClient : WebsocketClient, new()
    {
        /// <summary>
        ///     urlForClients 
        /// </summary>
        public string UrlForClients { get; private set; } = "ws://localhost:80";

        /// <summary>
        ///     urlForClients 
        /// </summary>
        public string[] UrlOfExchangers { get; private set; }

        /// <summary>
        ///     underline web
        /// </summary>
        private WebSocketServer _serverForClients;

        /// <summary>
        ///     clients of exchange server
        /// </summary>
        private List<TExchangerClient> _exchangeClients;

        /// <summary>
        ///    internal called when an Obj is initialized
        /// </summary>
        /// <param name="arg"></param>
        protected override void OnInit(ObjArg arg)
        {
            base.OnInit(arg);
            UrlForClients = (arg as ConnectorArg)?.UrlForClients;
            UrlOfExchangers = (arg as ConnectorArg)?.UrlOfExchangers;
            if(UrlOfExchangers.IsNullOrEmpty())
                throw new ArgumentException("UrlOfExchangers is null or empty");

            _serverForClients = new WebSocketServer(UrlForClients);
            _serverForClients.AddWebSocketService<TClientSession>("/Client");

            _exchangeClients = new List<TExchangerClient>();
            foreach (var url in UrlOfExchangers)
            {
                var client = Obj.Create<TExchangerClient>(new WebsocketClientArg(null, Uid.Create(), url), false);
                _exchangeClients.Add(client);
            }
        }

        /// <summary>
        ///     Invoked when obj started
        /// </summary>
        protected override void OnStart()
        {
            base.OnStart();
            _serverForClients.Start();

            foreach (var exchangeClient in _exchangeClients)
            {
                exchangeClient.ConnectAsync();
            }
        }

        /// <summary>
        ///    internal called when an Obj is to be destroyed
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();

            foreach (var exchangeClient in _exchangeClients)
            {
                exchangeClient.Close();
            }

            _serverForClients.Stop();
        }
    }
}
