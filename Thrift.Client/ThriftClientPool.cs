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


namespace Thrift.Client
{
    /// <summary>
    /// 回收连接时，能够过滤错误的连接
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ThriftClientPool<T> : IThriftClientPool<T> where T : class
    {
        private ConcurrentQueue<ThriftClient<T>> _clients = new ConcurrentQueue<ThriftClient<T>>();

        private ThriftClientConfig _config;
        private int _count = 0;//已创建的连接数量
        private object _lockHelper = new object();
        private object _lockPopHelper = new object();

        private Dictionary<string, string> _listPop = new Dictionary<string, string>();
        private HashSet<string> _hashErrorPop = new HashSet<string>();

        public ThriftClientPool(string sectionName, string serviceName)
        {
            _config = new ThriftClientConfig(sectionName, serviceName, UpdatePool);

            if (_config.ServiceConfig.IncrementalConnections < 0) throw new Exception("每次增长连接数不能小于0");
            if (_config.ServiceConfig.MinConnectionsNum < 0) throw new Exception("最小连接池不能小于0");
            if (_config.ServiceConfig.MinConnectionsNum > _config.ServiceConfig.MaxConnectionsNum) throw new Exception("最大连接池不能小于最小连接池");

            CreateConnections(_config.ServiceConfig.MinConnectionsNum);
        }

        /// <summary>
        /// 更新连接池，删除不可用的连接
        /// </summary>
        public void UpdatePool()
        {
            lock (_lockHelper)
            {
                int count = _clients.Count;
                for (int i = 0; i < count; i++)
                {
                    ThriftClient<T> client = null;
                    if (!_clients.TryDequeue(out client))
                        break;

                    if (!_config.ServiceConfig.Host.Contains(client.Host))
                    {
                        ThriftLog.Info("删除不可用的连接：" + client.Host);
                        client.Destroy(); //删除不可用的连接
                    }
                    else
                        client.Push(); //主动回收
                }
            }

            //回收连接时进行过滤
            lock (_lockPopHelper)
            {
                _hashErrorPop = new HashSet<string>();
                foreach (var item in _listPop)
                {
                    if (!_config.ServiceConfig.Host.Contains(item.Value))
                        _hashErrorPop.Add(item.Key);
                }
            }
        }

        /// <summary>
        /// 创建连接
        /// </summary>
        /// <param name="num"></param>
        private void CreateConnections(int num)
        {
            lock (_lockHelper)
            {
                for (int i = 0; i < num; ++i)
                {
                    if (_count >= _config.ServiceConfig.MaxConnectionsNum)
                        return;

                    var item = ThriftClientFactory.Create(_config.ServiceConfig, true);
                    if (item == null) return;
                    var token = System.Guid.NewGuid().ToString();
                    var client = new ThriftClient<T>(Tuple.Create(item.Item1, item.Item2 as T), this, item.Item3, token);

                    _clients.Enqueue(client);
                    _count++;
                    ThriftLog.Info("连接池数：" + _count);
                }
            }
        }

        /// <summary>
        /// 从连接池返回一个可用连接
        /// </summary>
        /// <returns></returns>
        public ThriftClient<T> Pop()
        {
            ThriftClient<T> client = GetOrCreateConnection(false);
            if (client != null) return client;

            int count = 0;
            while (true)
            {
                System.Threading.Thread.Sleep(10);
                if (count++ <= 3)
                    client = GetOrCreateConnection(false);
                else
                    client = GetOrCreateConnection(true);

                if (client != null) return client;

                if (count >= _config.ServiceConfig.PoolTimeout / 10) //获取可用连接超时
                    return null;
            }
        }

        /// <summary>
        /// 回收一个连接
        /// </summary>
        /// <param name="client"></param>
        public void Push(Tuple<TTransport, T> client, string host, string token)
        {
            lock (_lockPopHelper)
            {
                _listPop.Remove(token);
            }

            //错误的连接
            if (_hashErrorPop.Contains(token))
            {
                ThriftLog.Info("回收错误的连接:" + token);
                _hashErrorPop.Remove(token);

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
                return;
            }

            //超过最大空闲
            if (_clients.Count() >= _config.ServiceConfig.MaxConnectionsIdle)
            {
                ThriftLog.Info($"当前连接数：{_count},超过最大空闲数：{_config.ServiceConfig.MaxConnectionsIdle}，如果此条信息过多，请增加最大空闲数");
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
                return;
            }

            _clients.Enqueue(new ThriftClient<T>(client, this, host, token));
        }

        public void Destroy(string token)
        {
            lock (_lockPopHelper)
            {
                _listPop.Remove(token);
            }
            _count--;
        }

        /// <summary>
        /// 返回连接或创建
        /// </summary>
        /// <returns></returns>
        private ThriftClient<T> GetOrCreateConnection(bool create)
        {
            //lock (_lockHelper)
            //{
            //}

            ThriftClient<T> client = null;

            if (_clients.TryDequeue(out client))
            {
                lock (_lockPopHelper)
                {
                    _listPop.Add(client.Token, client.Host);
                    return client;
                }
            }

            if (create)
                CreateConnections(_config.ServiceConfig.IncrementalConnections);
            return null;
        }
    }
}
