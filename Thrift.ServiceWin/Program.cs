using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
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
            ////生成使用代码
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "生成目录");
            Thrift.IDLHelp.Help help = new Thrift.IDLHelp.Help();
            help.Create(filePath, typeof(Thrift.Test.IGameService2), "Thrift.Test.Thrift", "ThriftTestThrift");

            //启动服务
            LogHelper.Info("start");
            Thrift.Server.Server.Start();

            Console.WriteLine("按做任意键关闭");
            Console.ReadLine();

            Console.WriteLine("关闭中...");
            Thrift.Server.Server.Stop();
            Console.WriteLine("关闭完成");
        }
    }
}
