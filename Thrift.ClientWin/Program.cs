using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Thrift.Client;
using Thrift.Protocol;
using Thrift.Transport;
using System.Net;

namespace Thrift.ClientWin
{
    class Program
    {
        static void Main(string[] args)
        {

            while (true)
            {
                try
                {


                    using (var svc = ThriftClientManager<ThriftTest.GameThriftService.Client>.GetClient("GameThriftService"))
                    {

                        Console.WriteLine("Get:" + Newtonsoft.Json.JsonConvert.SerializeObject(svc.Client.Get(1)));
                        Console.WriteLine("GetALL:" + Newtonsoft.Json.JsonConvert.SerializeObject( svc.Client.GetALL()));
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                System.Threading.Thread.Sleep(2000);
            }

            Console.ReadLine();
        }
    }
}
