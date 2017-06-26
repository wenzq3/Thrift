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
using System.IO;
using System.Text.RegularExpressions;
using Thrift.Test.Thrift;
using System.Net.Http;

namespace Thrift.ClientWin
{
    class Program
    {
        private static int _count = 0;
        static void Main(string[] args)
        {
            ThreadPool.SetMinThreads(100, 100);

            Thrift.Client.ThriftLog._eventInfo = (x) => { LogHelper.Info(x); };
            Thrift.Client.ThriftLog._eventError = (x) => { LogHelper.Error(x); };

            int test_console = int.Parse(ConfigurationManager.AppSettings["test_console"]);
            int test_count = int.Parse(ConfigurationManager.AppSettings["test_count"]);
            int test_th = int.Parse(ConfigurationManager.AppSettings["test_th"]);
            int test_sleep = int.Parse(ConfigurationManager.AppSettings["test_sleep"]);

            while (_count++ < test_count)
            {
                System.Threading.Thread.Sleep(test_sleep);
                try
                {
                    using (var svc = ThriftClientManager<ThriftTestThrift.Client>.GetClient("ThriftTestThrift"))
                    {
                        string guid = System.Guid.NewGuid().ToString();
                        var guid2 = svc.Client.GetGuid(guid);
                        if (guid != guid2)
                            Console.WriteLine($"{guid} != {guid2}");
                        if (test_console == 1)
                            Console.WriteLine("true:" + DateTime.Now);
                        LogHelper.Fatal("true");
                        //        Console.WriteLine("true:"+DateTime.Now.ToString());
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Error("", ex);
                    Console.WriteLine("false:");
                }
            }

            Console.Read();


            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var threads = new List<Thread>();
            for (int i = 0; i < test_th; i++)
            {
                threads.Add(new Thread(() =>
                {
                    while (_count++ < test_count)
                    {
                        System.Threading.Thread.Sleep(test_sleep);
                        try
                        {

                            using (var svc = ThriftClientManager<ThriftTestThrift.Client>.GetClient("ThriftTestThrift"))
                            {
                                string guid = System.Guid.NewGuid().ToString();
                                var guid2 = svc.Client.GetGuid(guid);
                                if (guid != guid2)
                                    Console.WriteLine($"{guid} != {guid2}");
                                if (test_console == 1)
                                    Console.WriteLine("true:" + DateTime.Now);

                                //        Console.WriteLine("true:"+DateTime.Now.ToString());
                            }

                            //using (TTransport transport = new TSocket("192.168.1.179", 9021))
                            //using (TProtocol protocol = new TBinaryProtocol(transport))
                            //using (ThriftTestThrift.Client client = new ThriftTestThrift.Client(protocol))
                            //{
                            //    transport.Open();
                            //    string guid = System.Guid.NewGuid().ToString();
                            //    var guid2 = client.get2(guid);
                            //    if (guid != guid2)
                            //        Console.WriteLine($"{guid} != {guid2}");
                            //}
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Error("", ex);
                            Console.WriteLine("false:" + ex.StackTrace);
                        }
                    }
                }));
            }

            for (int i = 0; i < test_th; i++)
            {
                System.Threading.Thread.Sleep(100);
                threads[i].Start();
            }

            while (_count < test_count) ;
            stopwatch.Stop();

            Console.WriteLine(stopwatch.ElapsedMilliseconds);
            Console.WriteLine("over:" + _count);


            Console.ReadLine();
        }
    }
}
