using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Thrift.Service;
using Thrift.Test;

namespace Thrift.ServiceWin
{
    class Program
    {
        static void Main(string[] args)
        {
            //生成使用代码
            //string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "生成目录");
            //Thrift.IDLHelp.Help help = new Thrift.IDLHelp.Help();
            //////help.Create(filePath, typeof(Thrift.Test.IGameService2));
            //help.Create(filePath, typeof(Thrift.Test.IGameService), "Thrift.Test.Thrift", "ThriftTestThrift", "1.2.0");

            ////help.AnalyzeIDL(@"f:\doc\ToPSvr.thrift", @"f:\qvc\", "Tcy.ToPSvr.Thrift", "1.0.1");
            //Console.ReadLine();
            ////启动服务

            LogHelper.Info("start");

            //
            Thrift.Server.ThriftLog._eventInfo = (x) => { LogHelper.Info(x); };
            Thrift.Server.ThriftLog._eventError = (x) => { LogHelper.Error(x); };

            //统计方法执行时间
            Thrift.Server.Server._funcTime = (x, y, z) =>
            {
                Console.WriteLine($"执行方法完成：{x}({JsonSerializer(y)})  豪秒:{z}");
            };

            //统计方法异常
            Thrift.Server.Server._funcError = (x, y, z) =>
            {
             Console.WriteLine($"执行方法异常：{x}({JsonSerializer(y)})  异常:{z.Message}");
            };


            Thrift.Server.Server.Start();

            Console.WriteLine("按做任意键关闭");
            Console.ReadLine();

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
