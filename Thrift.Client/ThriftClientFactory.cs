using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
        static public Tuple<TTransport, object, string> Create(Config.Service config, List<string> errorHost = null)
        {
            try
            {
                string[] url = config.Host.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                if (url.Length == 0) return null;

                List<string> listUri = new List<string>();
                foreach (string item in url)
                {
                    var uri = item.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                    var length = uri.Length > 1 ? int.Parse(uri[1]) : 1;
                    int i = 0;
                    while (i++ < Math.Min(1, length))
                    {
                        listUri.Add(uri[0]);
                    }
                }

                if (listUri.Count == 0)
                    return null;

                int num = new Random().Next(0, listUri.Count);
                string host = listUri[num];

                ThriftLog.Info("创建连接：" + config.Host + "--" + host);

                TTransport transport = new TSocket(host.Split(':')[0], int.Parse(host.Split(':')[1]), config.Timeout);

                TProtocol protocol = new TBinaryProtocol(transport);

                string assemblyName = config.SpaceName;
                if (!string.IsNullOrEmpty(config.AssemblyName))
                    assemblyName = config.AssemblyName;

                return Tuple.Create(transport, Type.GetType($"{config.SpaceName}.{config.ClassName}+Client,{assemblyName}", true)
               .GetConstructor(new Type[] { typeof(TProtocol) })
                .Invoke(new object[] { protocol }), host);
            }
            catch (Exception ex)
            {
                throw new Exception("ThriftClientFactory 创建实例异常:"+ex.Message);
            }
        }
    }
}
