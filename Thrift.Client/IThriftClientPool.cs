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
    ///  
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IThriftClientPool<T> where T : class
    {
        /// <summary>
        /// 从连接池返回一个可用连接
        /// </summary>
        /// <returns></returns>
        ThriftClient<T> Pop();

        /// <summary>
        /// 回收一个连接
        /// </summary>
        /// <param name="client"></param>
        void Push(Tuple<TTransport, T> client, string host, string token);

        void Destroy(string token);

    }
}
