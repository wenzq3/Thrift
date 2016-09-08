using System;
using System.Configuration;

namespace Thrift.Client.Config
{
    /// <summary>
    /// service config
    /// </summary>
    public class Service : ConfigurationElement
    {
        /// <summary>
        /// 服务名称
        /// </summary>
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get { return (string)this["name"]; }
        }

        /// <summary>
        /// 实现类名
        /// </summary>
        [ConfigurationProperty("typeName", IsRequired = true)]
        public string TypeName
        {
            get { return (string)this["typeName"]; }
        }

        /// <summary>
        /// 最小连接数
        /// </summary>
        [ConfigurationProperty("minConnectionsNum", IsRequired = false, DefaultValue = 10)]
        public int MinConnectionsNum
        {
            get { return (int)this["minConnectionsNum"]; }
        }

        /// <summary>
        /// 最大连接数
        /// </summary>
        [ConfigurationProperty("maxConnectionsNum", IsRequired = false, DefaultValue = 100)]
        public int MaxConnectionsNum
        {
            get { return (int)this["maxConnectionsNum"]; }
        }

        /// <summary>
        /// 每次增加连接数
        /// </summary>
        [ConfigurationProperty("incrementalConnections", IsRequired = false, DefaultValue = 5)]
        public int IncrementalConnections
        {
            get { return (int)this["incrementalConnections"]; }
        }

        /// <summary>
        /// 返回超时值，毫秒单位
        /// </summary>
        [ConfigurationProperty("timeout", IsRequired = false, DefaultValue = 10000)]
        public int Timeout
        {
            get { return (int)(this["timeout"]); }
        }

        /// <summary>
        /// Host
        /// </summary>
        [ConfigurationProperty("host", IsRequired = false, DefaultValue ="")]
        public string Host
        {
            get { return (string)this["host"]; }
            set { this["host"] = value; }
        }

        /// <summary>
        /// 返回可用连接超时
        /// </summary>
        [ConfigurationProperty("poolTimeout", IsRequired = false, DefaultValue = 10000)]
        public int PoolTimeout
        {
            get { return (int)this["poolTimeout"]; }
        }

        /// <summary>
        /// zookeeper 配置 获取地址与端口号
        /// </summary>
        [ConfigurationProperty("ZookeeperConfig", IsRequired = false, DefaultValue = null)]
        public ZookeeperConfig ZookeeperConfig
        {
            get { return this["ZookeeperConfig"] as ZookeeperConfig; }
        }
    }
}