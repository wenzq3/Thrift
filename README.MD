###Thrift.Test 测试接口

###Thrift.IDLHelp 代码生成类库 

###Thrift.Server thrift服务端类库

###Thrift.Service 实现接口

###Thrift.ServiceWin 1.生成代码 2.启动服务


> &nbsp;

>//生成使用代码

>//string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "生成目录");

>//Thrift.IDLHelp.Help help = new Thrift.IDLHelp.Help();

>//Console.WriteLine(help.Create(filePath, typeof(Thrift.Test.IGameService), "ThriftTest", "GameThriftService"));
>
> 

>//Console.ReadLine();

> &nbsp;

> //启动服务


>Thrift.Server.Server.Start();

###Thrift.Client thrift客户端类库

###Thrift.ClientWin thrift客户端使用

>单服务器下

>using (var svc = ThriftClientManager<ThriftTest.GameThriftService.Client>.GetClientSimple("GameThriftService"))
                    {
 svc.Client.Get(1);
 svc.Client.GetALL();
                    }
                    
>集群模式下

>using (var svc = ThriftClientManager<ThriftTest.GameThriftService.Client>.GetClient("GameThriftService"))
                    {
 svc.Client.Get(1);
 svc.Client.GetALL();
                    }

### Dependency 存放生成的测试dll