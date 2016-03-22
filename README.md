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
    // for .net45
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
    
    // for .net35
    public class ChatSession : DispatchableSession
    {
        public override void HandlePush(IDataProtocol ps, Action<bool> cb)
        {
            var more = ps as DefaultDataProtocol;
            // do your custom handle
            var result = true/* your custom handle result*/;
            cb(result);
        }
    
        public override void HandleRequest(IDataProtocol rq, Action<NetResult> cb)
        {
            var more = ps as DefaultDataProtocol;
            // do your custom handle
            var result = NetResult.Success/* your custom handle result*/;
            cb(result);
        }
    }
```

####Request/Push api
Reqeust means that we need a response from the remote peer, but push just tell the remote peer something happened,<br>
needn't an response.<br>

So, in the Request api, a callback provided(.net3.5) or a Task<NetResult>(.net4.5) async returned, this callback or<br>
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

For example, socket4net's default data protocol:
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

#### Security
By default, socket4net doesn't encrypt your transformed data, but you can change this by provide an `Encryptor`/`Decryptor` method
pair in your custom session's `OnInit` method. 

For example:
```C#
    var key = Encoding.Default.GetBytes("12345678");
    var des = DES.Create();

    var encryptor = des.CreateEncryptor(key, key);
    Encoder = bytes => encryptor.TransformFinalBlock(bytes, 0, bytes.Length);

    var decryptor = des.CreateDecryptor(key, key);
    Decoder = bytes => decryptor.TransformFinalBlock(bytes, 0, bytes.Length);
```
##Question
QQ group ：http://jq.qq.com/?_wv=1027&k=VptNja
<br>e-mail : lizs4ever@163.com
