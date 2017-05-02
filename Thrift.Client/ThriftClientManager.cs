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
        static private readonly Dictionary<string, ThriftClientPool<T>> _ditPool = new Dictionary<string, ThriftClientPool<T>>();
        static private readonly Dictionary<string, ThriftClientNoPool<T>> _ditNoPool = new Dictionary<string, ThriftClientNoPool<T>>();
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

            if (_ditPool.ContainsKey(serviceName))
                return _ditPool[serviceName].Pop();

            lock (lockHp)
            {
                if (_ditPool.ContainsKey(serviceName))
                    return _ditPool[serviceName].Pop();

                _ditPool.Add(serviceName, new ThriftClientPool<T>(sectionName, serviceName));
                return _ditPool[serviceName].Pop();
            }
        }

        static public ThriftClient<T> GetClientNoPool(string serviceName)
        {
            return GetClientNoPool("thriftClient", serviceName);
        }

        static public ThriftClient<T> GetClientNoPool(string sectionName,string serviceName)
        {
            if (string.IsNullOrEmpty(serviceName)) throw new ArgumentNullException("serviceName");

            if (_ditNoPool.ContainsKey(serviceName))
                return _ditNoPool[serviceName].Pop();

            lock (lockHp)
            {
                if (_ditNoPool.ContainsKey(serviceName))
                    return _ditNoPool[serviceName].Pop();

                _ditNoPool.Add(serviceName, new ThriftClientNoPool<T>(sectionName, serviceName));
                return _ditNoPool[serviceName].Pop();
            }
        }

        //using (TTransport transport = new TSocket("192.168.1.179", 9021))
        //using (TProtocol protocol = new TBinaryProtocol(transport))
        //using (GameRoomThrift.Client client = new GameRoomThrift.Client(protocol))
        //{
        //    transport.Open();

        //}
    }
}
