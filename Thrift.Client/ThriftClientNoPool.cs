using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Thrift.Protocol;
using Thrift.Transport;
using ZooKeeperNet;

namespace Thrift.Client
{
    /// <summary>
    /// 不使用连接池
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ThriftClientNoPool<T> : IThriftClientPool<T> where T : class
    {
        private ConcurrentQueue<ThriftClient<T>> _clients = new ConcurrentQueue<ThriftClient<T>>();

        private ThriftClientConfig _config;
        private int _count = 0;//已创建的连接数量
        private object _lockHelper = new object();
        private object _lockPopHelper = new object();

        private Dictionary<string, string> _listPop = new Dictionary<string, string>();
        private HashSet<string> _hashErrorPop = new HashSet<string>();

        public ThriftClientNoPool(string sectionName, string serviceName)
        {
            _config = new ThriftClientConfig(sectionName, serviceName, null);

            if (_config == null)
                throw new Exception($"{sectionName} 结点 {serviceName} 不存在");

            if (_config.Config.IncrementalConnections < 0) throw new Exception("每次增长连接数不能小于0");
            if (_config.Config.MinConnectionsNum < 0) throw new Exception("最小连接池不能小于0");
            if (_config.Config.MinConnectionsNum > _config.Config.MaxConnectionsNum) throw new Exception("最大连接池不能小于最小连接池");
        }


        /// <summary>
        /// 从连接池返回一个可用连接
        /// </summary>
        /// <returns></returns>
        public ThriftClient<T> Pop()
        {
            if (_count >= _config.Config.MaxConnectionsNum)
            {
          //      ThriftLog.Error("count:" + _count);
                return null;
            }
            var item = ThriftClientFactory.Create(_config.Config, false);
            if (item == null) return null;
            //     var token = System.Guid.NewGuid().ToString();
            var client = new ThriftClient<T>(Tuple.Create(item.Item1, item.Item2 as T), this, item.Item3, "");

            _count++;
            ThriftLog.Error("count:" + _count);
            return client;
        }

        /// <summary>
        /// 回收一个连接
        /// </summary>
        /// <param name="client"></param>
        public void Push(Tuple<TTransport, T> client, string host, string token)
        {
            try
            {
                client.Item1.Close();
                client.Item1.Dispose();
                client = null;
            }
            catch (Exception ex)
            {
                ThriftLog.Error(ex.Message + ex.StackTrace);
            }
            _count--;
            ThriftLog.Error("count:" + _count);
        }

        public void Destroy(string token)
        {
            _count--;
            ThriftLog.Error("count:" + _count);
        }
    }
}
