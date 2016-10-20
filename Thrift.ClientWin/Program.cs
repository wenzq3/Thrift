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
                    using (var svc = ThriftClientManager<ThriftTestThrift.Client>.GetClient("ThriftTestThrift"))
                    {
                        Console.WriteLine("GetALL:" + svc.Client.get2());
                        //        Console.WriteLine("Get:" + Newtonsoft.Json.JsonConvert.SerializeObject(svc.Client.Get(1)));

                        Console.WriteLine("true");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("false" + ex.Message);
                }
                System.Threading.Thread.Sleep(5000);
            }

            int het = 10;

    
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

                        using (var svc = ThriftClientManager<ThriftTestThrift.Client>.GetClient("ThriftTestThrift"))
                        {
                            Console.WriteLine("GetALL:" + svc.Client.get2());
                            //        Console.WriteLine("Get:" + Newtonsoft.Json.JsonConvert.SerializeObject(svc.Client.Get(1)));

                            Console.WriteLine("true");
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
