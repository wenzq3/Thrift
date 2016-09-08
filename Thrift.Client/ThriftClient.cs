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

        public ThriftClient(Tuple<TTransport, T> client, ThriftClientPool<T> clientPool)
        {
            _client = client;
            _clientPool = clientPool;
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
                    ThriftLog.Info("destory:" + ex.Message);
                    this.Destroy();
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
                    //if (_client != null)
                    //{
                    //    _clientPool.Push(_client);
                    //    _client = null;
                    //}
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
            }
        }
    }
}
