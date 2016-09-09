using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thrift.Protocol;
using Thrift.Transport;

namespace Thrift.Client
{
    /// <summary>
    /// thrift client factory
    /// </summary>
    static public class ThriftClientFactory
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        static public Tuple<TTransport, object, string> Create(Config.Service config)
        {
            string[] url = config.Host.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string host = url[0];

            if (url.Length > 1)
            {
                int num = new Random().Next(0, url.Length);
                host = url[num];
            }
            Console.WriteLine(config.Host+"--"+ host);
            TTransport transport = new TSocket(host.Split(':')[0], int.Parse(host.Split(':')[1]), config.Timeout);
            TProtocol protocol = new TBinaryProtocol(transport);

            return Tuple.Create(transport, Type.GetType(config.TypeName, true)
           .GetConstructor(new Type[] { typeof(TProtocol) })
            .Invoke(new object[] { protocol }), host);
        }
    }
}
