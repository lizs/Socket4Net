C# 异步socket框架

文档地址：https://github.com/lizs/Socket4Net/wiki

文档尚不完善，以ChatS/ChatC Sample为主要参考。

点击链接加入群【Socket4Net】：http://jq.qq.com/?_wv=1027&k=VptNja
e-mail : lizs4ever@163.com


快速开始

日志
    
    日志可自定义，只要实现Logger.ILog接口即可。在日志调用之前的某个时间点，需要初始日志实例，如下所示：	
    Logger.Instance = new DefaultLogger();
    Socket4Net内核提供了DefaultLogger，它实际上只是一个简陋的Console日志。Socket4Net在Logger模块中提供了对Log4net的日志封装：CustomLog.Log4Net，引用该模块你就能使用log4net作为你的日志实例。

Rpc会话

    创建一个ExampleSesssion，从RpcSession派生，重写：
    1、Tuple<bool, byte[]> HandleRequest(ushort route, byte[] param)
    2、bool HandlePush(ushort route, byte[] param)
    方法1、2分别在收到Request和Push请求时由底层调用。同样，RocSession提供Push和Request接口，用来支持推送、请求调用。    
    注意：Push请求无需对端响应处理结果，而Request请求需要对端响应处理结果（处理结果为Tuple<bool, byte[])，只有在收到对端响应之后才完成本次Rpc。

    Rpc消息由底层确保在LogicService线程派发，无需上层做线程同步处理。
	
服务器示例

 // 创建服务器
 var server = new Server<ExampleSession>("0.0.0.0", 5000);
 // 监听事件
 server.EventSessionClosed += (session, reason) => Logger.Instance.InfoFormat("{0} disconnected by {1}", session.Id, reason);
 server.EventSessionEstablished += session => Logger.Instance.InfoFormat("{0} connected", session.Id);
 // 启动服务器
 // 注意：该服务器独享自己的网络服务和逻辑服务，故传入参数为null
 server.Start(null, null);
 // 结束服务器
 server.Stop();
	
    在命令行输入：telnet 127.0.0.1 5000 来测试服务器是否已运行。
	
多服务器示例	
    
    每个服务器实例都能自由选择共享或独享逻辑服务或网络服务。若需共享某服务，则需要单独创建并启动该服务，并在服务器实例结束自己的生命周期之后主动停止该服务。Socket4Net内核提供了两种内置的服务：NetService和LogicService，使用这两个服务可以满足绝大多数业务需求。	
	
 // 创建服务器
 var serverA = new Server<ExampleSession>("0.0.0.0", 5000);
 var serverB = new Server<ExampleSession>("0.0.0.0", 5001);
 var serverC = new Server<ExampleSession>("0.0.0.0", 5002);

 // 监听事件
 // ...

 // 启动服务器
 // 让serverA拥有自己独立的网络服务和逻辑服务，故传入参数为null
 serverA.Start(null, null);

 // 创建独立服务
 var logicService = new LogicService { Capacity = 10000, Period = 10 };
 var netService = new NetService { Capacity = 10000, Period = 10 };
 logicService.Start();
 netService.Start();

 // serverB和serverC共享服务
 serverB.Start(netService, logicService);
 serverC.Start(netService, logicService);

 // 结束服务器
 serverA.Stop();
 serverB.Stop();
 serverC.Stop();
 netService.Stop();
 logicService.Stop();	

客户端示例
     
 客户端接口调用与服务器别无二致：	
 var client = new Client<ExampleSession>(“127.0.0.1”, 5000);
 client.EventSessionClosed +=(session, reason) => Logger.Instance.InfoFormat("{0} disconnected by {1}", session.Id, reason);
 client.EventSessionEstablished += session => Logger.Instance.InfoFormat("{0} connected", session.Id);
 client.Start(null, null);
 client.Stop();
 同样客户端亦可以拥有多个实例，它们之间可以共享或独享网络、逻辑服务。	
