//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Thrift.Transport;

//namespace Thrift.Client
//{
//    public class ThriftClientSimple<T> : IDisposable where T : class
//    {
//        private Tuple<TTransport, T> _client;
//        private ThriftClientConfig _config;

//        public ThriftClientSimple(string sectionName, string serviceName)
//        {
//            _config = new ThriftClientConfig(sectionName, serviceName,null);

//            var item = ThriftClientFactory.Create(_config.Config);
//            if (item == null) return;

//            _client = Tuple.Create(item.Item1, item.Item2 as T);
//            _client.Item1.Open();
//        }

//        public T Client
//        {
//            get { return _client.Item2; }
//        }

//        bool disposed = false;

//        ~ThriftClientSimple()
//        {
//            Dispose(false);
//        }
//        protected void Dispose(bool disposing)
//        {
//            if (!disposed)
//            {
//                if (disposing)
//                {
//                    if (_client != null)
//                    {
//                        _client.Item1.Dispose();
//                        _client = null;
//                    }
//                }
//                disposed = true;
//            }
//        }
//        public void Dispose()
//        {
//            Dispose(true);
//            GC.SuppressFinalize(this);
//        }
//    }
//}
