using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thrift.ServiceWin
{
    class Program
    {
        static void Main(string[] args)
        {
            //生成使用代码
            //string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "生成目录");
            //Thrift.IDLHelp.Help help = new Thrift.IDLHelp.Help();
            //Console.WriteLine(help.Create(filePath, typeof(Thrift.Test.IGameService), "ThriftTest", "GameThriftService"));
            //Console.ReadLine();

            //启动服务
            //Thrift.Server.ThriftLog._eventInfo = (x) => { LogHelper.Info(x); };
            //Thrift.Server.ThriftLog._eventError = (x) => { LogHelper.Error(x); };
            Thrift.Server.Server.Start();

            Console.ReadLine();
        }
    }
}
