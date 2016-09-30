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
            string filePath = @"F:\ThriftTest\Thrift.ClientWin\11.cs";

            FileStream fs = new FileStream(filePath, FileMode.Open);//打开文件
            StreamReader tr = new StreamReader(fs, Encoding.Default);

            string str = tr.ReadToEnd();
            tr.Close();
            fs.Close();
            Console.WriteLine(str);


            Regex regex = new Regex("throw new TApplicationException\\(TApplicationException.ExceptionType.MissingResult,.*unknown result\"\\);", RegexOptions.IgnoreCase);
            var newSource = regex.Replace(str, "return null;");



            fs = new FileStream(filePath, FileMode.Create);//创建文件，存在则覆盖
            StreamWriter sw = new StreamWriter(fs);//写入

            sw.Write(newSource);
            sw.Close();
            fs.Close();

            while (true)
            {
                try
                {
                    using (var svc = ThriftClientManager<ThriftTest.GameThriftService.Client>.GetClient("GameThriftService"))
                    {
                        var vv = svc.Client.Get(1);
                        var vv2 = svc.Client.GetALL();

                        var aa = svc.Client.aa();

                        var bb = svc.Client.bb();

                        var cc = svc.Client.cc();


                        var ss = svc.Client.ss();
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
                            svc.Client.Get(1);
                            svc.Client.GetALL();
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
