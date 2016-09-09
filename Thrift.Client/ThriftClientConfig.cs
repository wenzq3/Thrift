using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thrift.Client.Config;
using ZooKeeperNet;

namespace Thrift.Client
{
    public class ThriftClientConfig : IWatcher
    {
        private Config.Service _config;
        private string _sectionName, _serviceName;
        private string _configPath;

        public ThriftClientConfig(string sectionName, string serviceName)
        {
            _configPath = ConfigurationManager.AppSettings["ThriftClientConfigPath"];
            _sectionName = sectionName;
            _serviceName = serviceName;
            _config = GetConfig(true);
        }

        /// <summary>
        /// 配置信息
        /// </summary>
        /// <returns></returns>
        public Config.Service Config
        {
            get { return _config; }
        }

        /// <summary>
        /// 获取配置
        /// </summary>
        /// <param name="sectionName"></param>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        private Config.Service GetConfig(bool watch)
        {
            if (string.IsNullOrEmpty(_sectionName))
                throw new ArgumentNullException("sectionName");

            if (string.IsNullOrEmpty(_serviceName))
                throw new ArgumentNullException("serviceName");

            Config.ThriftConfigSection config = null;
            if (string.IsNullOrEmpty(_configPath))
                config = ConfigurationManager.GetSection(_sectionName) as Config.ThriftConfigSection;
            else
            {
                config = ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap
                {
                    ExeConfigFilename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _configPath)
            }, ConfigurationUserLevel.None).GetSection(_sectionName) as Config.ThriftConfigSection;
            }


            foreach (Config.Service service in config.Services)
            {
                if (service.Name != _serviceName) continue;

                if (service.ZookeeperConfig == null || service.ZookeeperConfig.ZHost == "")
                    return service;

                bool isConnZk = SetServerConfig(service, watch);
                if (!isConnZk)
                {
                    new System.Threading.Thread(() =>
                    {
                        while (!isConnZk)
                        {
                            System.Threading.Thread.Sleep(service.ZookeeperConfig.ConnectInterval);
                            isConnZk = SetServerConfig(service, watch);
                        }
                    }).Start();
                }

                return service;
            }

            return null;
        }

        private bool SetServerConfig(Config.Service service, bool watch)
        {
            try
            {
                var zk = ZookeeperHelp.CreateClient(service.ZookeeperConfig.ZHost, service.ZookeeperConfig.SessionTimeout, null, "TestUser:123456");

                if (zk == null)
                    throw new Exception($"Zookeeper服务 {service.ZookeeperConfig.ZHost} 连接失败");

                var children = ZookeeperHelp.GetChildren(zk, service.ZookeeperConfig.ZNodeParent);
                if (children != null && children.Count > 0)
                    service.Host = string.Join(",", children);

                if (watch)
                    WatchServer(zk, service.ZookeeperConfig.ZNodeParent);

                return true;
            }
            catch (Exception ex)
            {
                ThriftLog.Error(ex.Message + ex.StackTrace);
                return false;
            }
        }

        /// <summary>
        /// 监听服务注册
        /// </summary>
        private void WatchServer(ZooKeeper zk, string znode)
        {
            new System.Threading.Thread(() =>
            {
                bool isRegister = false;
                while (!isRegister)
                {
                    isRegister = ZookeeperWatcherHelp.Register(zk, znode, null, (@event, nodeData) =>
                     {
                             _config = GetConfig(false);
                     });

                    System.Threading.Thread.Sleep(10000);
                }
            }).Start();
        }

        public void Process(WatchedEvent @event)
        {
            //服务节点删除了，需要重新获取服务地址
            if (@event.Type == EventType.NodeDeleted)
            {
                _config = GetConfig(false);
            }
        }
    }
}
