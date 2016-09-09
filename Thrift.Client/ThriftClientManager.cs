using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Thrift.Transport;

namespace Thrift.Client
{
    public static class ThriftClientManager<T> where T : class
    {
        static private readonly Dictionary<string, ThriftClientPool<T>> _dit = new Dictionary<string, ThriftClientPool<T>>();
        private static object lockHp = new object();

        static ThriftClientManager()
        {
        }

        static public ThriftClient<T> GetClient(string serviceName)
        {
            return GetClient("thriftClient", serviceName);
        }

        static public ThriftClient<T> GetClient(string sectionName, string serviceName)
        {
            if (string.IsNullOrEmpty(serviceName)) throw new ArgumentNullException("serviceName");

            if (_dit.ContainsKey(serviceName))
                return _dit[serviceName].Pop();

            lock (lockHp)
            {
                if (_dit.ContainsKey(serviceName))
                    return _dit[serviceName].Pop();

                _dit.Add(serviceName, new ThriftClientPool<T>(sectionName, serviceName));
                return _dit[serviceName].Pop();
            }
        }
    }
}
