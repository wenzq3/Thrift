﻿using System;
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
    /// 简单的连接池管理，（集群模式下，收到服务关闭通知时，不会删除所有的不可用连接，下次使用时可能报错)。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ThriftClientPoolSimple<T> where T : class
    {
        private ConcurrentQueue<Lazy<ThriftClientSimple<T>>> _clients = new ConcurrentQueue<Lazy<ThriftClientSimple<T>>>();

        private ThriftClientConfig _config;
        private int _count = 0;//已创建的连接数量
        private object lockHelper = new object();

        public ThriftClientPoolSimple(string sectionName, string serviceName)
        {
            _config = new ThriftClientConfig(sectionName, serviceName, UpdatePool);

            if (_config == null)
                throw new Exception($"{sectionName} 结点 {serviceName} 不存在");

            if (_config.Config.IncrementalConnections < 0) throw new Exception("每次增长连接数不能小于0");
            if (_config.Config.MinConnectionsNum < 0) throw new Exception("最小连接池不能小于0");
            if (_config.Config.MinConnectionsNum > _config.Config.MaxConnectionsNum) throw new Exception("最大连接池不能小于最小连接池");

            CreateConnections(_config.Config.MinConnectionsNum);
        }

        /// <summary>
        /// 更新连接池，删除不可用的连接
        /// </summary>
        public void UpdatePool()
        {
            int count = _clients.Count;
            for (int i = 0; i < count; i++)
            {
                Lazy<ThriftClientSimple<T>> client = null;
                if (!_clients.TryDequeue(out client))
                    break;

                if (!_config.Config.Host.Contains(client.Value.Host))
                {
                    ThriftLog.Info($"连接池中 删除不可用的连接： {client.Value.Host}");
                    client.Value.Destroy(); //删除不可用的连接
                }
                else
                    client.Value.Dispose();
            }
        }

        /// <summary>
        /// 创建连接
        /// </summary>
        /// <param name="num"></param>
        private void CreateConnections(int num)
        {
            lock (lockHelper)
            {
                for (int i = 0; i < num; ++i)
                {
                    if (_count >= _config.Config.MaxConnectionsNum)
                        return;

                    Lazy<ThriftClientSimple<T>> client = new Lazy<ThriftClientSimple<T>>(() =>
                    {
                        var item = ThriftClientFactory.Create(_config.Config);
                        return new ThriftClientSimple<T>(Tuple.Create(item.Item1, item.Item2 as T), this, item.Item3);
                    });

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
        public ThriftClientSimple<T> Pop()
        {
            ThriftClientSimple<T> client = GetOrCreateConnection(false);
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

                if (count >= _config.Config.PoolTimeout / 10) //获取可用连接超时
                    return null;
            }
        }

        /// <summary>
        /// 回收一个连接
        /// </summary>
        /// <param name="client"></param>
        public void Push(Tuple<TTransport, T> client, string host)
        {
            //超过最大空闲
            if (_clients.Count() >= _config.Config.MaxConnectionsIdle)
            {
                ThriftLog.Info($"当前连接数：{_count},超过最大空闲数：{_config.Config.MaxConnectionsIdle}，如果此条信息过多，请增加最大空闲数");
                try
                {
                    client.Item1.Close();
                    client = null;
                }
                catch (Exception ex)
                {
                    ThriftLog.Error(ex.Message + ex.StackTrace);
                }
                _count--;
                return;
            }

            _clients.Enqueue(new Lazy<ThriftClientSimple<T>>(() => new ThriftClientSimple<T>(client, this, host)));
        }

        public void Destroy()
        {
            _count--;
        }

        /// <summary>
        /// 返回连接或创建
        /// </summary>
        /// <returns></returns>
        private ThriftClientSimple<T> GetOrCreateConnection(bool create)
        {
            Lazy<ThriftClientSimple<T>> client = null;
            if (_clients.TryDequeue(out client))
                return client.Value;
            else
            {
                if (create)
                    CreateConnections(_config.Config.IncrementalConnections);
                return null;
            }
        }
    }
}
