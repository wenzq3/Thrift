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


namespace Thrift.Client
{
    /// <summary>
    /// 不使用连接池
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ThriftClientNoPool<T> : IThriftClientPool<T> where T : class
    {
        private ThriftClientConfig _config;
        private int _count = 0;//已创建的连接数量
        private string[] _host;
        private int _hostCount;
        private int _hostIndex = 0;

        public ThriftClientNoPool(string sectionName, string serviceName, string spaceName,string className)
        {
            _config = new ThriftClientConfig(sectionName, serviceName, UpdatePool, spaceName, className);

            _host = _config.ServiceConfig.Host.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            _hostCount = _host.Length;
            _hostIndex = 0;

            if (_config.ServiceConfig.IncrementalConnections < 0) throw new Exception("每次增长连接数不能小于0");
            if (_config.ServiceConfig.MinConnectionsNum < 0) throw new Exception("最小连接池不能小于0");
            if (_config.ServiceConfig.MinConnectionsNum > _config.ServiceConfig.MaxConnectionsNum) throw new Exception("最大连接池不能小于最小连接池");
        }

        /// <summary>
        /// 更新连接池
        /// </summary>
        public void UpdatePool()
        {
            ThriftLog.Info("UpdatePool:" + _config.ServiceConfig.Host);
            _host = _config.ServiceConfig.Host.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            _hostCount = _host.Length;
            _hostIndex = 0;
        }

        /// <summary>
        /// 从连接池返回一个可用连接
        /// </summary>
        /// <returns></returns>
        public ThriftClient<T> Pop()
        {
            if (_count >= _config.ServiceConfig.MaxConnectionsNum)
            {
                ThriftLog.Error("连接池达到最大数:" + _count);
                return null;
            }
            if (_hostCount == 0) return null;

            _config.ServiceConfig.Host = _host[_hostIndex % _hostCount];

            var item = ThriftClientFactory.Create(_config.ServiceConfig, false);
            if (item == null) return null;
            var client = new ThriftClient<T>(Tuple.Create(item.Item1, item.Item2 as T), this, item.Item3, "");

            _count++;
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
        }

        public void Destroy(string token)
        {
            _count--;
            _hostIndex++;
        }
    }
}
