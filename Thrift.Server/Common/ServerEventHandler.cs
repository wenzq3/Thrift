using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Thrift;
using Thrift.Protocol;
using Thrift.Server.Config;
using Thrift.Transport;

namespace Thrift.Server
{
    public class ServerEventHandler : TServerEventHandler
    {
        private static ConcurrentDictionary<int, string> _ditIP = new ConcurrentDictionary<int, string>();

        /// <summary>
        /// 获取客户端IP
        /// </summary>
        /// <returns></returns>
        public static string GetClientIP()
        {
            int id = System.Threading.Thread.CurrentThread.ManagedThreadId;
            return GetClientIP(id);
        }

        /// <summary>
        /// 获取客户端IP
        /// </summary>
        /// <param name="currentThreadId">当前托管线程的唯一标识符</param>
        /// <returns></returns>
        public static string GetClientIP(int currentThreadId)
        {
            if (_ditIP.ContainsKey(currentThreadId))
                return _ditIP[currentThreadId];
            return "";
        }

        // 创建Context的时候，触发
        public object createContext(TProtocol input, TProtocol output)
        {
            TSocket socket = (TSocket)input.Transport;
            IPEndPoint ip = (IPEndPoint)socket.TcpClient.Client.RemoteEndPoint;
            var id = System.Threading.Thread.CurrentThread.ManagedThreadId;
            _ditIP[id] = ip.Address.ToString();

            return null;
        }

        //删除Context的时候，触发
        public void deleteContext(object serverContext, TProtocol input, TProtocol output)
        {
            Console.WriteLine("del");
            var id = System.Threading.Thread.CurrentThread.ManagedThreadId;
            string value;
            _ditIP.TryRemove(id, out value);
        }

        //服务成功启动后执行
        public void preServe()
        {
            ThriftLog.Info("Thrift Start server ...");
        }

        //每调用一次方法，就会触发一次
        public void processContext(object serverContext, TTransport transport)
        {
            ThriftLog.Info("processContext");
        }
    }
}
