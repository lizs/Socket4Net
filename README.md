# Socket4Net

C# async socket network framework
support .net3.5（for unity3d）、.net4.0 、.net4.5
qq : 670958929, e-mail : lizs4ever@163.com

quick start

ChatS、ChatC is sample TcpServer and TcpClient

创建及运行服务器（基础）
var server = new TcpServer<RpcSession>(ip, port);
server.Startup();
其中RpcSession是框架内置Session，用来支持RPC。

SessionMgr.EventSessionClosed += (session, reason) => Console.Writeline(session.Id + " closed");
SessionMgr.EventSessionEstablished += session => Console.Writeline(session.Id + " established");

这时候，服务器已经运行，telnet该服务器即可看到会话建立的消息。


RPC调用（进阶）
Rpc分三类：Request, Notify和Response

public enum RpcType
{
	/// <summary>
	/// 通知
	/// 无需等待对端响应
	/// </summary>
	Notify,
	/// <summary>
	/// 请求
	/// 需要等待对端响应才能完成本次Rpc
	/// </summary>
	Request,
	/// <summary>
	/// 响应Request
	/// </summary>
	Response,
}

RpcSession负责rpc请求，其核心接口如下：
// 请求对端响应
public void Request(RpcRoute route, object proto, Action<bool, byte[]> cb)；
// 通知对端
public void Notify(RpcRoute route, object proto)；
// 通知所有客户端
public static void NotifyAll(RpcRoute route, object proto)；

RpcRoute（2字节）枚举所有的路由id，是一个rpc请求的唯一标识；proto是某个ProtoBuf对象的实例；对于Request调用，还需传递一个回调cb。

当RpcSession收到一个rpc之后，需要对于的RpcHandler来处理，否则该rpc失败。RpcHandler由RpcHost管理，RpcHost需要明确自己能够处理哪些rpc（RpcRoute），这是通过调用如下虚方法来完成的：
protected abstract void RegisterRpcHandlers();

所以，需要派生类来重写RegisterRpcHandlers方法.比如在ChatS中Chater就派生自RpcHost,
public class Chater : RpcHost
{
	...
	protected override void RegisterRpcHandlers()
	{
		RequestsHandlers.Add(RpcRoute.GmCmd, HandleGmRequest);
		NotifyHandlers.Add(RpcRoute.Chat, HandleChatNotify);
	}
	
	private object HandleGmRequest(byte[] bytes){...}
    private bool HandleChatNotify(byte[] bytes){...}
}

这样Chater就能处理RpcRoute.GmCmd、RpcRoute.Chat了。注意：HandleGmRequest返回了一个ProtoBuf的实例，该实例是响应对端的数据。

StaService、NetService（进阶）
StaService、NetService旨在提供单线程处理环境，其中StaService用在服务器逻辑处理中，而NetService用在网络数据收发处理中。
