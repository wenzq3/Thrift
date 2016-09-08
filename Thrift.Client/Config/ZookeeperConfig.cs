using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thrift.Client.Config
{
    public class ZookeeperConfig : ConfigurationElement
    {
        /// <summary>
        /// zookeeper 服务地址
        /// </summary>
        [ConfigurationProperty("zhost", IsRequired = true)]
        public string ZHost
        {
            get { return (string)this["zhost"]; }
        }

        /// <summary>
        /// zookeeper 服务地址
        /// </summary>
        [ConfigurationProperty("sessionTimeout", IsRequired = false, DefaultValue = 5000)]
        public int SessionTimeout
        {
            get { return (int)this["sessionTimeout"]; }
        }

        /// <summary>
        /// zookeeper 重连间隔时间
        /// </summary>
        [ConfigurationProperty("connectInterval", IsRequired = false, DefaultValue = 10000)]
        public int ConnectInterval
        {
            get { return (int)this["connectInterval"]; }
        }
        

        /// <summary>
        /// 父级节点路径 
        /// </summary>
        [ConfigurationProperty("znodeParent", IsRequired = true)]
        public string ZNodeParent
        {
            get { return (string)this["znodeParent"]; }
        }
    }
}
