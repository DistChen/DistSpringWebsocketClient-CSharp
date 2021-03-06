# DistSpringWebsocketClient-CSharp
连接 [Spring WebSocket](http://docs.spring.io/spring/docs/4.3.0.RC2/spring-framework-reference/htmlsingle/#websocket-intro) ，并发布、订阅消息的 C# 客户端。
-----------------------------------

如果你用过[WebSocket](https://en.wikipedia.org/wiki/WebSocket) 的话，你会觉得用来做推送等消息服务真轻松，同时你也会意识到因为其太简单，导致很多事情都需要自己处理。为了简化二次处理的过程，[Spring](http://spring.io) 提供了一个 [Spring WebSocket](http://docs.spring.io/spring/docs/4.3.0.RC2/spring-framework-reference/htmlsingle/#websocket-intro) 模块，此模块用 [Stomp 协议](https://stomp.github.io/stomp-specification-1.2.html)来处理消息，使得我们可以用发布、订阅的模式来使用WebSocket。

这里要弄明白的是：[Spring WebSocket](http://docs.spring.io/spring/docs/4.3.0.RC2/spring-framework-reference/htmlsingle/#websocket-intro) 仅仅只是用了 [Stomp 协议](https://stomp.github.io/stomp-specification-1.2.html) 协议的消息格式，并不是说 [Spring WebSocket](http://docs.spring.io/spring/docs/4.3.0.RC2/spring-framework-reference/htmlsingle/#websocket-intro) 自己是一个 Stomp Server，所以是无法使用 [Stomp 协议](https://stomp.github.io/stomp-specification-1.2.html) 提供的各种 Client 来进行连接并使用的（[Stomp.js](http://jmesnil.net/stomp-websocket/doc/)除外）。因此如果想用 C# 等语言连接上 [Spring WebSocket](http://docs.spring.io/spring/docs/4.3.0.RC2/spring-framework-reference/htmlsingle/#websocket-intro)，只要保证发送的消息符合 [Stomp Frame](https://stomp.github.io/stomp-specification-1.2.html#STOMP_Frames) 的格式就行了。
### [Stomp Frame](https://stomp.github.io/stomp-specification-1.2.html#STOMP_Frames) 示例
        COMMAND
        header1:value1
        header2:value2

        Body^@

  
1、先来看看 stomp.js 中如何连接、发布、订阅
-----------------------------------
### 创建一个 Client 对象（此对象的定义及实例化由 Stomp.js 提供）
        var client = Stomp.client("ws://127.0.0.1:8080/dist");
### 进行连接
        client.connect({login:"distchen",passcode:"pass"}, function(frame) {
            console.log(frame);
        });
### 订阅某个 topic
        var subscribe = client.subscribe('/topic/greet/1', function(frame){
            console.log(frame);
        });
### 发布消息到某个topic
        client.send("/send/greet/1", {}, "hello world");
### 取消订阅某个 topic
        subscribe.unsubscribe("/send/greet/1", {}, "hello world");
从上面可以看到，使用 Spring WebSocket 和 Stomp.js 可以使我们以非常优雅的方式来使用 WebSocket。很遗憾的是，这种优雅的方式目前只有 Stomp.js 才能用。为了说明这种优雅的方式其实在任何语言中都是可以使用的，因此才有了这个项目，只是简单说明怎样用其它语言处理 stomp.js 处理的事情，以C#为例，其它语言都是类似的处理。

2、我用 C# 如何处理
-----------------------------------
此类库中，同样提供了一个Client 类(Dist.SpringWebsocket.Client)。
### 创建一个 Client 对象
        // 第二个参数为一个委托，当服务不可用或者异常等均会调用，传递相应的代码和消息(异常)
        Client client = new Client("ws://127.0.0.1:8080/dist", new Receive(delegate(StompFrame frame)
        {
            txtContent.Text += frame.Code.ToString() + ":" + frame.Content + Environment.NewLine;
            
        }));
### 进行连接
        //连接成功后同样会返回相应的 StompFrame 
        Dictionary<string, string> headers = new Dictionary<string, string>();
        headers.Add("login", "DistChen");
        headers.Add("passcode", "pass");
        this.client.Connect(headers, new Receive(delegate(StompFrame frame)
        {
            txtContent.Text += frame.Code.ToString() + ":" + frame.Content + Environment.NewLine;
        }));
### 订阅某个 topic
         //订阅此topic后，之后发往此topic的消息均会调用此委托
        this.client.Subscribe("/topic/greet/1", new Receive(delegate(StompFrame frame)
        {
            txtContent.Text += frame.Code.ToString() + ":" + JObject.Parse(frame.Content).GetValue("content") + Environment.NewLine;
        }));
### 发布消息到某个topic
        this.client.Send("/send/greet/1", "hello world");
### 取消订阅某个 topic
        this.client.UnSubscribe("/topic/greet/1");

从上面可以看到，使用了此类库后，处理方式与 stomp.js 很类似。不仅可以保证使用 spring websocket,而且还提供了很优雅的方式来使用。

>浏览器与PC端通信<br>
>![image](https://raw.githubusercontent.com/DistChen/DistSpringWebsocketClient-CSharp/master/dist/1.png "浏览器与PC端通信")<br>
>服务不可用时，相应的处理机制<br>
>![image](https://raw.githubusercontent.com/DistChen/DistSpringWebsocketClient-CSharp/master/dist/2.png "服务不可用时，相应的处理机制")
