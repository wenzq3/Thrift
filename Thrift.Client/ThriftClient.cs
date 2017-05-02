using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thrift.Transport;

namespace Thrift.Client
{
    public class ThriftClient<T> : IDisposable where T : class
    {
        private Tuple<TTransport, T> _client;
        private IThriftClientPool<T> _clientPool;
        private string _host;
        private string _token;

        public ThriftClient(Tuple<TTransport, T> client, IThriftClientPool<T> clientPool, string host, string token)
        {
            _client = client;
            _clientPool = clientPool;
            _host = host;
            _token = token;
        }

        /// <summary>
        /// 当前令牌
        /// </summary>
        public string Token
        {
            get { return _token; }
        }

        /// <summary>
        /// 当前连接的主机
        /// </summary>
        public string Host
        {
            get { return _host; }
        }

        public T Client
        {
            get
            {
                try
                {
                    if (!_client.Item1.IsOpen)
                    {
                        _client.Item1.Close();
                        _client.Item1.Open();
                    }
                    return _client.Item2;
                }
                catch (Exception ex)
                {
                    //    ThriftLog.Info($"销毁连接： {_host} {ex.Message}");
                    Destroy();
                    throw new Exception($"ThriftClient 连接异常 {_host}");
                }
            }
        }

        bool disposed = false;

        ~ThriftClient()
        {
            Dispose(false);
        }
        protected void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (_client != null)
                    {
                        _clientPool.Push(_client, _host, _token);
                        _client = null;
                    }
                }
                disposed = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 主动回收
        /// </summary>
        public void Push()
        {
            if (_client != null)
            {
                _clientPool.Push(_client, _host, _token);
                _client = null;
            }
            disposed = true;
        }

        /// <summary>
        /// 销毁
        /// </summary>
        public void Destroy()
        {
            if (_client != null)
            {
                _client.Item1.Dispose();
                _client = null;
                _clientPool.Destroy(_token);
            }
        }
    }
}
