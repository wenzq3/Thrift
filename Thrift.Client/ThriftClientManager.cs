using org.apache.zookeeper;
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
            ZooKeeper.CustomLogConsumer = new ZookeeperLog();
            ZooKeeper.LogToFile = false;
            ZooKeeper.LogToTrace = false;
        }

        static public ThriftClient<T> GetClient(string serviceName)
        {
            return GetClient("thriftClient", serviceName);
        }

        static public ThriftClient<T> GetClient(string sectionName, string serviceName)
        {
            if (string.IsNullOrEmpty(serviceName)) throw new ArgumentNullException("serviceName");

            var type = typeof(T);
            string key = $"{serviceName}_{type.Namespace}_{type.ReflectedType.Name}";

            if (_ditPool.ContainsKey(key))
                return _ditPool[key].Pop();

            lock (lockHp)
            {
                if (_ditPool.ContainsKey(key))
                    return _ditPool[key].Pop();

                _ditPool.Add(key, new ThriftClientPool<T>(sectionName, serviceName, type.Namespace, type.ReflectedType.Name));
                return _ditPool[key].Pop();
            }
        }

        static public ThriftClient<T> GetClientNoPool(string serviceName)
        {
            return GetClientNoPool("thriftClient", serviceName);
        }

        static public ThriftClient<T> GetClientNoPool(string sectionName, string serviceName)
        {
            if (string.IsNullOrEmpty(serviceName)) throw new ArgumentNullException("serviceName");

            var type = typeof(T);
            string key = $"{serviceName}_{type.Namespace}_{type.ReflectedType.Name}";

            if (_ditNoPool.ContainsKey(key))
                return _ditNoPool[key].Pop();

            lock (lockHp)
            {
                if (_ditNoPool.ContainsKey(key))
                    return _ditNoPool[key].Pop();

                _ditNoPool.Add(key, new ThriftClientNoPool<T>(sectionName, serviceName, type.Namespace, type.ReflectedType.Name));
                return _ditNoPool[key].Pop();
            }
        }
    }
}
