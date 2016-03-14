socket4net
======================
A C# asynchronous socket library.

* easy to use
* cross-platform(.net/mono)
* efficient

##requirements
> protobuf-net

##Getting started
###Server<br>
```C#
    //startup launcher
    var launcher = Obj.New<Launcher>(LauncherArg.Default, true);
    //starup server
    var server = Obj.New<Server<ChatSession>>(new ServerArg(null, "127.0.0.1", 9527), true);
    //stop server
    server.Destroy();
    //stop launcher
    launcher.Destroy();
```
###Client<br>
client is the same as server
    
###Message handle
All messages are dispatched in the `DispatchableSession`, it has two abstract methods:
```C#
      public abstract Task<NetResult> HandleRequest(IDataProtocol rq);
      public abstract Task<bool> HandlePush(IDataProtocol ps);
```
you need implement them in your custom session.
for example:
```C#
    public class ChatSession : DispatchableSession
    {
        public override Task<bool> HandlePush(IDataProtocol ps)
        {
            var more = ps as DefaultDataProtocol;
            // do your custom handle
            return Task.FromResult(true);
        }
    
        public override Task<NetResult> HandleRequest(IDataProtocol rq)
        {
            var more = ps as DefaultDataProtocol;
            // do your custom handle
            return Task.FromResult(NetResult.Success);
        }
    }
```

####Request/Push api
Reqeust means that we need an response from the remote peer, but push just tell the remote peer something happened,<br>
needn't an response.<br>

So, in the Request api, an callback provided(.net3.5) or an Task<NetResult>(.net4.5) async returned, this callback or<br>
Task<NetResult> tell the caller whether this request is responsed correctly.
```C#
      // multi push
      void MultiCast<T>(T proto, IEnumerable<ISession> sessions) where T : IDataProtocol;
      void Broadcast<T>(T proto) where T : IDataProtocol;
      
      // reqeust
      Task<NetResult> RequestAsync<T>(T proto) where T : IDataProtocol;
      void RequestAsync<T>(T proto, Action<bool, byte[]> cb) where T : IDataProtocol;
      
      // push
      void Push<T>(T proto) where T : IDataProtocol;
```
Every proto is an protobuf-net's `ProtoContact`, it can be an socket4net's `DefaultDataProtocol`, or any custom protocol<br> that implemented from `IDataProtocol`.
    
for example, socket4net's default data protocol:
```C#
    [ProtoContract]
    public class DefaultDataProtocol : IDataProtocol
    {
        [ProtoMember(1)]
        public short Ops { get; set; }
        [ProtoMember(2)]
        public byte[] Data { get; set; }
    }
```

##Question
QQ group ：http://jq.qq.com/?_wv=1027&k=VptNja
<br>e-mail : lizs4ever@163.com
