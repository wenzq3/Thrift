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
        private Config.Service _serviceConfig;
        private string _sectionName, _serviceName;
        private string _configPath;
        private ZooKeeper _zk;

        private string _defaultHost = "";//默认地址
        private Action _updateHostDelegate = null; //服务主机更改通知

        public ThriftClientConfig(string sectionName, string serviceName, Action updateHostDelegate)
        {
            _configPath = ConfigurationManager.AppSettings["ThriftClientConfigPath"];
            _sectionName = sectionName;
            _serviceName = serviceName;
            _serviceConfig = GetServiceConfig(true);
            _updateHostDelegate = updateHostDelegate;
        }

        /// <summary>
        /// 配置信息
        /// </summary>
        /// <returns></returns>
        public Config.Service ServiceConfig
        {
            get { return _serviceConfig; }
        }

        /// <summary>
        /// 获取配置
        /// </summary>
        /// <param name="sectionName"></param>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        private Config.Service GetServiceConfig(bool isFirst)
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

                if (service.ZookeeperConfig == null || service.ZookeeperConfig.Host == "")
                    return service;

                bool isConnZk = SetServerConfig(service, isFirst);
                if (!isConnZk)
                {
                    new System.Threading.Thread(() =>
                    {
                        while (!isConnZk)
                        {
                            System.Threading.Thread.Sleep(service.ZookeeperConfig.ConnectInterval);
                            isConnZk = SetServerConfig(service, isFirst);
                        }
                    }).Start();
                }

                return service;
            }

            return null;
        }

        private bool SetServerConfig(Config.Service service, bool isFirst)
        {
            try
            {
                if (_zk == null)
                    _zk = ZookeeperHelp.CreateClient(service.ZookeeperConfig.Host, service.ZookeeperConfig.SessionTimeout, this, "");

                if (_zk == null)
                    throw new Exception($"Zookeeper服务 {service.ZookeeperConfig.Host} 连接失败");

                _defaultHost = service.Host;
                var children = ZookeeperHelp.GetChildren(_zk, service.ZookeeperConfig.NodeParent);
                if (children != null && children.Count > 0)
                    service.Host = string.Join(",", children);

                if (!isFirst) //首次连接，不需要执行更新方法。
                {
                    if (_updateHostDelegate != null)
                        _updateHostDelegate();
                }

                WatchServer(_zk, service.ZookeeperConfig.NodeParent);

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
            var isRegister = ZookeeperWatcherHelp.Register(zk, znode, null, (@event, nodeData) =>
              {
                  try
                  {
                      var children = ZookeeperHelp.GetChildren(_zk, _serviceConfig.ZookeeperConfig.NodeParent);
                      if (children != null && children.Count > 0)
                          _serviceConfig.Host = string.Join(",", children);
                      else
                          _serviceConfig.Host = _defaultHost;

                      if (_updateHostDelegate != null)
                          _updateHostDelegate();
                  }
                  catch (Exception ex)
                  {
                      ThriftLog.Error("GetChildren:" + ex.Message + ex.StackTrace);
                  }
              });

            ThriftLog.Info("WatchServer :" + zk + ":" + znode + ":" + isRegister);
        }

        public void Process(WatchedEvent @event)
        {
            ThriftLog.Info("WatchedEvent :" + @event.State.ToString());
            if (@event.State == KeeperState.Expired)
            {
                ThriftLog.Info(" 重新连接zk");
                //_zk.Dispose();
                _zk = null;
                _serviceConfig = GetServiceConfig(false);
            }
        }
    }
}
