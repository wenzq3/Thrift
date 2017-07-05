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
using GameThrift;

namespace Thrift.ClientWin
{
    class Program
    {
        private static int _count = 0;
        static void Main(string[] args)
        {
            //using (TTransport transport = new TSocket("127.0.0.1", 9001))
            //using (TProtocol protocol = new TBinaryProtocol(transport))
            //using (GameService.Client client = new GameService.Client(protocol))
            //{
            //    transport.Open();
            //    var info = client.GetGameInfo(1001);
            //    Console.WriteLine($"{info.GameId},{info.GameName}");
            //}

            //记录信息与错误日志
            Thrift.Client.ThriftLog._eventInfo = (x) => { Console.WriteLine("info:" + x); };
            Thrift.Client.ThriftLog._eventError = (x) => { Console.WriteLine("error:" + x); };

            //使用连接池，保持连接
            using (var svc = ThriftClientManager<ThriftTestThrift.Client>.GetClient("ThriftTestThrift"))
            {
                try
                {
                    var info = svc.Client.GetTestInfo();
                    Console.WriteLine("true:" + Newtonsoft.Json.JsonConvert.SerializeObject(info));
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    if (svc != null)
                        svc.Destroy();
                }
            }
            //不保持连接
            using (var svc = ThriftClientManager<ThriftTestThrift.Client>.GetClientNoPool("ThriftTestThrift"))
            {
                try
                {
                    var info = svc.Client.GetTestInfo();
                    Console.WriteLine("true:" + Newtonsoft.Json.JsonConvert.SerializeObject(info));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    if (svc != null)
                        svc.Destroy();
                }
            }

            int test_console = int.Parse(ConfigurationManager.AppSettings["test_console"]);
            int test_count = int.Parse(ConfigurationManager.AppSettings["test_count"]);
            int test_th = int.Parse(ConfigurationManager.AppSettings["test_th"]);
            int test_sleep = int.Parse(ConfigurationManager.AppSettings["test_sleep"]);

            while (_count++ < test_count)
            {
                try
                {
                    using (var svc = ThriftClientManager<ThriftTestThrift.Client>.GetClient("ThriftTestThrift"))
                    {
                        var info = svc.Client.GetTestInfo();
                        Console.WriteLine("true:" + Newtonsoft.Json.JsonConvert.SerializeObject(info));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                System.Threading.Thread.Sleep(test_sleep);
            }

            Console.Read();


            ThreadPool.SetMinThreads(100, 100);
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
                                var info = svc.Client.GetTestInfo();
                                //        Console.WriteLine("true:"+DateTime.Now.ToString());
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
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
