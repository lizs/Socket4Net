using System;
using System.Net;
using Core.Service;

namespace Core.Net.TCP
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

        bool IsLogicServiceShared { get; }
        bool IsNetServiceShared { get; }

        void Start(IService net, IService logic);
        void Stop();

        void PerformInLogic(Action action);
        void PerformInLogic<TParam>(Action<TParam> action, TParam param);

        void PerformInNet(Action action);
        void PerformInNet<TParam>(Action<TParam> action, TParam param);
    }

    public interface IPeer<out TSession, out TLogicService, out TNetService> : IPeer
        where TSession : class, ISession, new()
        where TNetService : IService, new()
        where TLogicService : ILogicService, new()
    {
        event Action<TSession, SessionCloseReason> EventSessionClosed;
        event Action<TSession> EventSessionEstablished;
        event Action EventPeerClosing;

        TLogicService LogicService { get; }
        TNetService NetService { get; }
    }
}