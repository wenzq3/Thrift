using org.apache.zookeeper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thrift.Client.Config;
using static org.apache.zookeeper.Watcher.Event;

namespace Thrift.Client
{
    public class ThriftClientConfig : Watcher
    {
        private Config.Service _serviceConfig;
        private string _sectionName, _serviceName;
        private string _configPath;
        private ZooKeeper _zk;

        private string _spaceName;
        private string _className;

        private string _defaultHost = "";//默认地址
        private Action _updateHostDelegate = null; //服务主机更改通知
        private bool _firstGetConfig = true;//第一次加载

        public ThriftClientConfig(string sectionName, string serviceName, Action updateHostDelegate, string spaceName = "", string className = "")
        {
            _spaceName = spaceName;
            _className = className;

            _configPath = ConfigurationManager.AppSettings["ThriftClientConfigPath"];
            _sectionName = sectionName;
            _serviceName = serviceName;
            _serviceConfig = GetServiceConfig();
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
        private Config.Service GetServiceConfig()
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

                if (string.IsNullOrEmpty(service.SpaceName))
                    service.SpaceName = _spaceName;
                if (string.IsNullOrEmpty(service.ClassName))
                    service.ClassName = _className;

                if (service.ZookeeperConfig == null || service.ZookeeperConfig.Host == "")
                    return service;

                bool isConnZk = SetServerConfig(service);
                _firstGetConfig = false;
                if (!isConnZk)
                {
                    new System.Threading.Thread(() =>
                    {
                        while (!isConnZk)
                        {
                            System.Threading.Thread.Sleep(service.ZookeeperConfig.ConnectInterval);
                            isConnZk = SetServerConfig(service);
                        }
                    }).Start();
                }

                return service;
            }

            return null;
        }

        private bool SetServerConfig(Config.Service service)
        {
            try
            {
                if (_zk == null)
                {
                    _zk = new ZooKeeper(service.ZookeeperConfig.Host, service.ZookeeperConfig.SessionTimeout, this);

                    int count = 0;
                    while (_zk.getState() != ZooKeeper.States.CONNECTED)
                    {
                        if (count++ > 50)
                            throw new Exception("ZooKeeper 连接失败");
                        System.Threading.Thread.Sleep(100);
                    }
                }

                _defaultHost = service.Host;
                var children = _zk.getChildrenAsync(service.ZookeeperConfig.NodeParent, this).Result.Children;
                if (children != null && children.Count > 0)
                    service.Host = string.Join(",", children);

                if (!_firstGetConfig) //首次连接，不需要执行更新方法。
                {
                    if (_updateHostDelegate != null)
                        _updateHostDelegate();
                }
                return true;
            }
            catch (Exception ex)
            {
                ThriftLog.Error(ex.Message + ex.StackTrace);
                return false;
            }
        }

        public override async Task process(WatchedEvent watchedEvent)
        {
            Console.WriteLine("WatchedEvent:" + watchedEvent.getState().ToString() + ":" + watchedEvent.get_Type().ToString());
            if (watchedEvent.getState() == KeeperState.Expired)
            {
                ThriftLog.Info(" 重新连接zk");
                await _zk.closeAsync();
                _zk = null;
                _serviceConfig = GetServiceConfig();
                return;
            }

            try
            {
                if (watchedEvent.get_Type() == EventType.NodeChildrenChanged)
                {
                    var data = await _zk.getChildrenAsync(_serviceConfig.ZookeeperConfig.NodeParent, this);
                    var children = data.Children;

                    if (children != null && children.Count > 0)
                        _serviceConfig.Host = string.Join(",", children);
                    else
                        _serviceConfig.Host = _defaultHost;

                    if (_updateHostDelegate != null)
                        _updateHostDelegate();
                }
            }
            catch (Exception ex)
            {
                ThriftLog.Error(ex.Message + ex.StackTrace);
                _zk = null;
                _serviceConfig = GetServiceConfig();
                ThriftLog.Info(" 重新连接zk2");
            }

            return;
        }
    }
}
