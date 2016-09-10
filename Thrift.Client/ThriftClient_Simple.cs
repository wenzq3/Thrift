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
        private ThriftClientPool<T> _clientPool;
        private string _host;

        public ThriftClient(Tuple<TTransport, T> client, ThriftClientPool<T> clientPool, string host)
        {
            _client = client;
            _clientPool = clientPool;
            _host = host;
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
                    ThriftLog.Info($"销毁连接： {_host} {ex.Message}");
                    Destroy();
                    return null;
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
                        _clientPool.Push(_client, _host);
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
        /// 销毁
        /// </summary>
        public void Destroy()
        {
            if (_client != null)
            {
                _client = null;
                _clientPool.Destroy();
                System.GC.Collect();
            }
        }
    }
}
