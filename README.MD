###Thrift.Test 测试接口

###Thrift.IDLHelp 代码生成类库 

###Thrift.Server thrift服务端类库

###Thrift.Service 实现接口

###Thrift.ServiceWin 1.生成代码 2.启动服务


> &nbsp;

>//生成使用代码

>//string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "生成目录");

>//Thrift.IDLHelp.Help help = new Thrift.IDLHelp.Help();

>//help.Create(filePath, typeof(Thrift.Test.IGameService), "Thrift.Test.Thrift", "ThriftTestThrift", "1.2.0");
>
> 


> &nbsp;

> //启动服务


> // Thrift.Server.ThriftLog._eventInfo = (x) => { LogHelper.Info(x); };
> // Thrift.Server.ThriftLog._eventError = (x) => { LogHelper.Error(x); };

###统计方法执行时间
> Thrift.Server.Server._funcTime = (x, y, z) =>
 {
 Console.WriteLine($"执行方法完成：{x}({JsonSerializer(y)})  豪秒:{z}");
 };

###统计方法异常
> Thrift.Server.Server._funcError = (x, y, z) =>
{
 Console.WriteLine($"执行方法异常：{x}({JsonSerializer(y)})  异常:{z.Message}");
 };

> Thrift.Server.Server.Start();

###Thrift.Client thrift客户端类库

###Thrift.ClientWin thrift客户端使用

> // Thrift.Client.ThriftLog._eventInfo = (x) => { LogHelper.Info(x); };
> // Thrift.Client.ThriftLog._eventError = (x) => { LogHelper.Error(x); };

>不保持连接

>   using (var svc = ThriftClientManager<ThriftTestThrift.Client>.GetClientNoPool("ThriftTestThrift"))
                    {
 svc.Client.GetGuid("xx");
                    }
                    
>保持连接

>using (var svc = ThriftClientManager<ThriftTestThrift.Client>.GetClient("ThriftTestThrift"))
                    {
 svc.Client.GetGuid("xx");
                    }

### Dependency 存放生成的测试dll