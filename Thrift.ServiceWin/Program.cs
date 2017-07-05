using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using Thrift.Test;

namespace Thrift.ServiceWin
{
    class Program
    {
        static void Main(string[] args)
        {
            //var processor = new GameService.Processor(new GameThriftHandler());
            //var proFactory = new TBinaryProtocol.Factory();

            //TServerTransport serverTransport = new TServerSocket(9001);
            //TServer server = new TSimpleServer(processor, serverTransport, new TTransportFactory(), proFactory);
            //server.Serve();

            //help.AnalyzeIDL(@"f:\doc\ToPSvr.thrift", @"f:\qvc\", "Tcy.ToPSvr.Thrift", "1.0.1");

            ////生成使用代码

            //string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "生成目录");
            //IDLHelp.Help help = new IDLHelp.Help();
            //help.Create(filePath, typeof(ITestService), "Thrift.Test.Thrift", "ThriftTestThrift", "2.0.0");



            //记录信息与错误日志
            Thrift.Server.ThriftLog._eventInfo = (x) => { Console.WriteLine("info:" + x); };
            Thrift.Server.ThriftLog._eventError = (x) => { Console.WriteLine("error:" + x); };

            Thrift.Server.Server._funcTime = (x, y, z) =>
            {
                Console.WriteLine($"执行方法完成：{x}({JsonSerializer(y)})  豪秒:{z}");
            };

            Thrift.Server.Server._funcError = (x, y, z) =>
            {
                Console.WriteLine($"执行方法异常：{x}({JsonSerializer(y)})  异常:{z.Message}");
            };

            Console.WriteLine("服务启动");
            Thrift.Server.Server.Start();

            Console.WriteLine("按做任意键关闭");
            Console.Read();

            Console.WriteLine("关闭中...");
            Thrift.Server.Server.Stop();
            Console.WriteLine("关闭完成");
        }


        public static string JsonSerializer<T>(T t)
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            MemoryStream ms = new MemoryStream();
            ser.WriteObject(ms, t);
            string jsonString = Encoding.UTF8.GetString(ms.ToArray());
            ms.Close();
            return jsonString;
        }
    }
}
