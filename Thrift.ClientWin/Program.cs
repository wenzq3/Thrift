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

            while (true)
            {
                   using (var svc = ThriftClientManager<ThriftTestThrift.Client>.GetClientNoPool("ThriftTestThrift"))
              //          using (var svc = ThriftClientManager<ThriftTestThrift.Client>.GetClient("ThriftTestThrift"))
                {
                    try
                    {
                        _count++;

                        string guid = _count.ToString();
                        var guid2 = svc.Client.get2(guid);

                        if (guid != guid2)
                            Console.WriteLine("false==================================");
                        else
                            Console.WriteLine("true");

                    //    var ts = svc.Client.gettime();
                    }
                    catch (Exception ex)
                    {
                        if (svc != null)
                            svc.Destroy();
                        Console.WriteLine("false:" + ex.Message);
                    }
                    //       svc.Destroy();
                }
                System.Threading.Thread.Sleep(2000);
            }

            //预热连接池
            using (var svc = ThriftClientManager<ThriftTestThrift.Client>.GetClient("ThriftTestThrift"))
            {
                string guid = System.Guid.NewGuid().ToString();
                var guid2 = svc.Client.get2(guid);
                if (guid != guid2)
                    Console.WriteLine($"{guid} != {guid2}");
            }

            int len = 100;
            int thlen =4;

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var threads = new List<Thread>();
            for (int i = 0; i < thlen; i++)
            {
                threads.Add(new Thread(() =>
                {
                    while (_count++ < len)
                    {
                        try
                        {
                            //  {
                            //      string guid = System.Guid.NewGuid().ToString();

                            //      var guid2 = RequestHttpData("http://127.0.0.1:1005/1.aspx?sleep=10&msg=" + guid,"GET");
                            ////      var guid2 = HttpGet("http://127.0.0.1:1005/1.aspx?sleep=10&msg=" + guid).Result;

                            //      if (guid != guid2)
                            //          Console.WriteLine($"{guid} != {guid2}");

                            //  }

                            using (var svc = ThriftClientManager<ThriftTestThrift.Client>.GetClientNoPool("ThriftTestThrift"))
                            {
                                string guid = System.Guid.NewGuid().ToString();
                                var guid2 = svc.Client.get2(guid);
                                if (guid != guid2)
                                    Console.WriteLine($"{guid} != {guid2}");

                                var ts= svc.Client.gettime();
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
                            Console.WriteLine("false:" + ex.StackTrace);
                        }
                    }
                }));
            }

            for (int i = 0; i < thlen; i++)
            {
             System.Threading.Thread.Sleep(100);
                threads[i].Start();
            }

            while (_count < len) ;
            stopwatch.Stop();

            Console.WriteLine(stopwatch.ElapsedMilliseconds);
            Console.WriteLine("over:" + _count);


            Console.ReadLine();
        }



        public static async Task<string> HttpGet(string url, int timeOut = 3, Dictionary<string, string> headers = null)
        {
            var httpClient = new HttpClient();
            try
            {

                httpClient.Timeout = TimeSpan.FromSeconds(timeOut);

                if (headers != null)
                {
                    foreach (KeyValuePair<string, string> k in headers)
                        httpClient.DefaultRequestHeaders.Add(k.Key, k.Value);
                }

                var response = await httpClient.GetAsync(url);
                var buffer = await response.Content.ReadAsStreamAsync();

                string result;
                StreamReader reader;
                reader = new StreamReader(buffer);
                result = await reader.ReadToEndAsync();
                return result;
            }
            finally
            {
                httpClient.Dispose();
            }
        }

        /// <summary>
        /// 同步get post
        /// </summary>
        /// <param name="url"></param>
        /// <param name="requestMethod">get or post</param>
        /// <param name="postData">post 数据</param>
        /// <param name="headers"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static string RequestHttpData(string url, string requestMethod, string postData = "", Dictionary<string, string> headers = null, int timeout = 2000)
        {
            var request = WebRequest.Create(url) as HttpWebRequest;
            if (request == null)
            {
                return "";
            }
            request.Timeout = timeout;
            request.Method = requestMethod;
            request.KeepAlive = true;
            request.AllowAutoRedirect = false;
            request.ContentType = "application/x-www-form-urlencoded";

            if (headers != null)
            {
                foreach (KeyValuePair<string, string> k in headers)
                    request.Headers.Add(k.Key, k.Value);
            }

            if (!string.IsNullOrEmpty(postData))
            {
                byte[] postdatabtyes = Encoding.UTF8.GetBytes(postData);
                request.ContentLength = postdatabtyes.Length;
                Stream requeststream = request.GetRequestStream();
                requeststream.Write(postdatabtyes, 0, postdatabtyes.Length);
                requeststream.Close();
            }
            else
            {
                request.ContentLength = 0;
            }

            string result;
            using (var response = request.GetResponse() as HttpWebResponse)
            {
                if (response == null)
                {
                    return "";
                }
                StreamReader reader;
                reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);

                result = reader.ReadToEnd();
            }
            return result;
        }


    }
}
