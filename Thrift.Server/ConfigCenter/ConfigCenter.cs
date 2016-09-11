using Org.Apache.Zookeeper.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thrift.Server.Config;
using ZooKeeperNet;

namespace Thrift.Server
{
    public class ConfigCenter
    {
        /// <summary>
        /// 注册服务
        /// </summary>
        /// <param name="service"></param>
        public static RegeditConfig RegeditServer(Service service)
        {
            RegeditConfig _regeditConfig = new RegeditConfig(service);
            _regeditConfig.Init();
            return _regeditConfig;
        }
    }
}
