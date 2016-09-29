using System;
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
    public class BaseProcessor : TProcessor
    {
        private TProcessor _processor;
        private Service _service;

        public BaseProcessor(TProcessor processor, Service service)
        {
            _processor = processor;
            _service = service;
        }

        /** 
         * 该方法，客户端每调用一次，就会触发一次 
         */
        public bool Process(TProtocol iprot, TProtocol oprot)
        {
            TSocket socket = (TSocket)iprot.Transport;

            IPEndPoint ip = (IPEndPoint)socket.TcpClient.Client.RemoteEndPoint;

            if (!IpLimit.AllowConfig(ip.Address.ToString()))
            {
                ThriftLog.Info($"{ip.Address.ToString()} 连接被限制!");
                return false;
            }

            return _processor.Process(iprot, oprot);
        }
    }
}
