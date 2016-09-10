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
        private static string _zk_host = ConfigurationManager.AppSettings["ZookeeperHost"];
        private static int _zk_sessionTimeout = int.Parse(ConfigurationManager.AppSettings["ZookeeperSessionTimeout"]);
        private static string _zk_digest = ConfigurationManager.AppSettings["ZookeeperDigest"];

   //     private static RegeditConfig _regeditConfig = null;

        //private static ZooKeeper _zk = ZookeeperHelp.CreateClient(_zk_host, _sessionTimeout, null, _zk_digest);
        //private static List<ACL> _zk_Acl = ZookeeperHelp.GetACL(_zk_digest);

        ///// <summary>
        ///// IP限制
        ///// </summary>
        //public static void InitIpLimit()
        //{
        //    IpLimitConfig ipLimitConfig = new IpLimitConfig(_zk, ConfigurationManager.AppSettings["IpLimitZnode"]);
        //    ipLimitConfig.Init();
        //}

        /// <summary>
        /// 注册服务
        /// </summary>
        /// <param name="service"></param>
        public static RegeditConfig RegeditServer(Service service)
        {
            RegeditConfig _regeditConfig = new RegeditConfig(_zk_host, _zk_sessionTimeout, _zk_digest, service);
            _regeditConfig.Init();
            return _regeditConfig;
        }

        //public static void LogoutServer()
        //{
        //    if (_regeditConfig == null)
        //        return;

        //    _regeditConfig.Logout();
        //}
    }
}
