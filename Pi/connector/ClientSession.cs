using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using socket4net;

namespace Pi
{
    /// <summary>
    ///     client session
    ///     forward to backend servers
    /// </summary>
    public abstract class ClientSession : WebsocketSession
    {
        /// <summary>
        ///     unique id bound to client
        ///     THIS IS NOT SESSION ID!!!
        /// </summary>
        public string Uid { get; private set; }

        ///// <summary>
        /////     backend sessions map
        /////     mapping Uid to correct backend server
        ///// </summary>
        //private Dictionary<string, short> _backendSessionsMap = new Dictionary<string, short>();

        /// <summary>
        ///     handle request
        /// </summary>
        /// <param name="rq"></param>
        /// <returns></returns>
        public override async Task<RpcResult> OnRequest(IDataProtocol rq)
        {
            var dp = rq as Client2ConnectorDataProtocol;
            if(dp == null)
                return RpcResult.Failure;

            if (dp.Ops < 0)
            {
                // internal handle
                switch ((EClient2ConnectorOps)dp.Ops)
                {
                    case EClient2ConnectorOps.Bind:
                    {
                        return await DoBind(dp.Data);
                    }

                    default:
                    {
                        Close();
                        break;
                    }
                }
            }
            else
            {
                // forward
                if(!Bound)
                    return RpcResult.Failure;

                // get target backend server session
            }

            return RpcResult.Failure;
        }

        /// <summary>
        ///     handle push
        /// </summary>
        /// <param name="ps"></param>
        /// <returns></returns>
        public override Task<bool> OnPush(IDataProtocol ps)
        {
            return base.OnPush(ps);
        }

        private Task<RpcResult> DoBind(byte[] data)
        {
            // bind uid
            var proto = PiSerializer.Deserialize<BindProto>(data);
            Uid = proto.Uid;

            // distribute exchange

            return Task.FromResult(RpcResult.Failure);
        }

        private bool Bound => !string.IsNullOrWhiteSpace(Uid);
    }
}