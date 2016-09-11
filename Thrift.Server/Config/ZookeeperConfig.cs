using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thrift.Server.Config
{
    public class ZookeeperConfig : ConfigurationElement
    {
        /// <summary>
        /// zookeeper 服务地址
        /// </summary>
        [ConfigurationProperty("host", IsRequired = true)]
        public string Host
        {
            get { return (string)this["host"]; }
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
        /// 父级节点路径  RPC服务的注册上级节点
        /// </summary>
        [ConfigurationProperty("nodeParent", IsRequired = true)]
        public string NodeParent
        {
            get { return (string)this["nodeParent"]; }
        }

        /// <summary>
        /// digest
        /// </summary>
        [ConfigurationProperty("digest", IsRequired = false, DefaultValue = "")]
        public string Digest
        {
            get { return (string)this["digest"]; }
        }

    }
}
