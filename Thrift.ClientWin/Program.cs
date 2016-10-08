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
                        //   Console.WriteLine("GetALL:" + Newtonsoft.Json.JsonConvert.SerializeObject(svc.Client.GetALL()));
                        //        Console.WriteLine("Get:" + Newtonsoft.Json.JsonConvert.SerializeObject(svc.Client.Get(1)));

                        Console.WriteLine("true");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("false" + ex.Message);
                }
                System.Threading.Thread.Sleep(50000);
            }

            int het = 1000;

            //var stopwatch = new Stopwatch();
            //stopwatch.Start();

            //var countdown = new CountdownEvent(het);
            //ThreadPool.SetMinThreads(1000, 1000);
            //ThreadPool.SetMaxThreads(1000, 1000);

            //for (int i = 0; i < het; i++)
            //{

            //    ThreadPool.QueueUserWorkItem((obj) =>
            //    {

            //try
            //{
            //    System.Threading.Thread.Sleep(10);
            //    using (var svc = ThriftClientManager<ThriftTest.GameThriftService.Client>.GetClient("GameThriftService"))
            //    {
            //        svc.Client.Get(1);
            //        svc.Client.GetALL();
            //        //        Console.WriteLine("true");
            //    }
            //}
            //catch { Console.WriteLine("false"); }
            //finally
            //{
            //    countdown.Signal();
            //}

            //    });

            //}


            //while (!countdown.IsSet) ;

            //stopwatch.Stop();

            //Console.WriteLine(stopwatch.ElapsedMilliseconds);
            //Console.WriteLine("over");



            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var threads = new List<Thread>();
            var countdown = new CountdownEvent(het);
            for (int i = 0; i < het; i++)
            {
                threads.Add(new Thread(() =>
                {
                    try
                    {

                        using (var svc = ThriftClientManager<ThriftTest.GameThriftService.Client>.GetClient("GameThriftService"))
                        //        using (var svc = ThriftClientManager<ThriftTest.GameThriftService.Client>.GetClientSimple("GameThriftService"))
                        {
                            if (svc == null)
                                Console.WriteLine("svc is null");

                            System.Threading.Thread.Sleep(10);

                            //      Console.WriteLine("true");
                        }
                    }
                    catch (Exception ex) { Console.WriteLine("false:" + ex.StackTrace); }
                    finally
                    {
                        countdown.Signal();
                    }
                }));
            }

            for (int i = 0; i < het; i++)
            {
                threads[i].Start();
                //            System.Threading.Thread.Sleep(10);
            }

            while (!countdown.IsSet) ;
            stopwatch.Stop();

            Console.WriteLine(stopwatch.ElapsedMilliseconds);
            Console.WriteLine("over");


            Console.ReadLine();
        }
    }
}
