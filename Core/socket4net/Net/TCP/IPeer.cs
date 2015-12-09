using System;
using System.Net;

namespace socket4net
{
    /// <summary>
    /// 终端
    /// 对服务器、客户端的抽象
    /// </summary>
    public interface IPeer
    {
        string Ip { get; }
        ushort Port { get; }
        IPAddress Address { get; }
        EndPoint EndPoint { get; }

        SessionMgr SessionMgr { get; }
        
        ILogicService LogicService { get; }
        INetService NetService { get; }

        bool IsLogicServiceShared { get; }
        bool IsNetServiceShared { get; }

        event Action<ISession, SessionCloseReason> EventSessionClosed;
        event Action<ISession> EventSessionEstablished;
        event Action<string> EventErrorCatched;
        event Action EventPeerClosing;
        
        void PerformInLogic(Action action);
        void PerformInLogic<TParam>(Action<TParam> action, TParam param);

        void PerformInNet(Action action);
        void PerformInNet<TParam>(Action<TParam> action, TParam param);
    }
}