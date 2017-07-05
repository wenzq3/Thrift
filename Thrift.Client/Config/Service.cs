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

        /// 是否多服务
        /// </summary>
        [ConfigurationProperty("isMult", IsRequired = false, DefaultValue = false)]
        public bool IsMult
        {
            get { return (bool)this["isMult"]; }
        }

        /// <summary>
        /// 命名空间名称
        /// </summary>
        [ConfigurationProperty("spaceName", IsRequired = false)]
        public string SpaceName
        {
            get { return (string)this["spaceName"]; }
            set { this["spaceName"] = value; }
        }

        /// <summary>
        /// 实现类名称
        /// </summary>
        [ConfigurationProperty("className", IsRequired = false)]
        public string ClassName
        {
            get { return (string)this["className"]; }
            set { this["className"] = value; }
        }

        /// <summary>
        /// 程序集名称
        /// </summary>
        [ConfigurationProperty("assemblyName", IsRequired = false, DefaultValue = "")]
        public string AssemblyName
        {
            get { return (string)this["assemblyName"]; }
        }

        /// <summary>
        /// 最小连接数
        /// </summary>
        [ConfigurationProperty("minConnectionsNum", IsRequired = false, DefaultValue = 5)]
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
        /// 最大空闲连接数
        /// </summary>
        [ConfigurationProperty("maxConnectionsIdle", IsRequired = false, DefaultValue = 20)]
        public int MaxConnectionsIdle
        {
            get { return (int)this["maxConnectionsIdle"]; }
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