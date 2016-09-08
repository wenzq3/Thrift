//using System;
//using System.Collections.Generic;
//using System.Configuration;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using ZooKeeperNet;

//namespace Thrift.Server
//{
//    /// <summary>
//    /// IP限制 通过zookeeper 获取配置，动态更新。
//    /// </summary>
//    internal class IpLimitConfig
//    {
//        private  ZooKeeper _zk;
//        private  string _zk_node;

//        public IpLimitConfig(ZooKeeper zk, string zk_node)
//        {
//            _zk = zk;
//            _zk_node = zk_node;
//        }

//        public void Init()
//        {
//            var _task = new Task(ActiveUpdateIpLimit, 1000, 60000 * 60);
//            _task.Start();

//            PassiveUpdateIpLimit();
//        }

//        /// <summary>
//        /// 更改IP配置
//        /// </summary>
//        /// <param name="value"></param>
//        private  void UpdateIpLimit(string value)
//        {
//            string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigurationManager.AppSettings["IpLimitPath"]);

//            try
//            {
//                var config = ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap
//                {
//                    ExeConfigFilename = configFilePath
//                }, ConfigurationUserLevel.None);


//                if (!config.AppSettings.Settings.AllKeys.Contains("IpWhite"))
//                {
//                    config.AppSettings.Settings.Add("IpWhite", value);
//                }
//                else
//                {
//                    if (config.AppSettings.Settings["IpWhite"].Value != value)
//                    {
//                        config.AppSettings.Settings["IpWhite"].Value = value;
//                    }
//                }

//                config.Save(ConfigurationSaveMode.Minimal);
//            }
//            catch (Exception ex)
//            {
//         //       LogHelper.Info(ex.Message);
//            }
//        }

//        /// <summary>
//        /// 主动获取更新
//        /// </summary>
//        private void ActiveUpdateIpLimit()
//        {
//            var nodeData = ZookeeperHelp.GetData(_zk, _zk_node);
//            if (nodeData != null)
//            {
//                UpdateIpLimit(System.Text.Encoding.UTF8.GetString(nodeData));
//            }
//        }

//        /// <summary>
//        /// 被动获取更新
//        /// </summary>
//        private  void PassiveUpdateIpLimit()
//        {
//            bool isRegister = false;
//            while (!isRegister)
//            {
//                isRegister = ZookeeperWatcherHelp.Register(_zk, _zk_node, (@event, nodeData) =>
//                {
//                    if (nodeData != null)
//                    {
//                        UpdateIpLimit(System.Text.Encoding.UTF8.GetString(nodeData));
//                        //LogHelper.Info(System.Text.Encoding.UTF8.GetString(nodeData));
//                    }
//                }, null);

//                System.Threading.Thread.Sleep(10000);
//            }
//        }
//    }
//}
