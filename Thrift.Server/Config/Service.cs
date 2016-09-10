using System;
using System.Configuration;

namespace Thrift.Server.Config
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
        /// HandlerType
        /// </summary>
        [ConfigurationProperty("handlerType", IsRequired = true)]
        public string HandlerType
        {
            get { return (string)this["handlerType"]; }
        }

        /// <summary>
        /// ProcessType
        /// </summary>
        [ConfigurationProperty("processType", IsRequired = true)]
        public string ProcessType
        {
            get { return (string)this["processType"]; }
        }

        /// <summary>
        /// IfaceType
        /// </summary>
        [ConfigurationProperty("ifaceType", IsRequired = true)]
        public string IfaceType
        {
            get { return (string)this["ifaceType"]; }
        }
        /// <summary>
        /// 最小连接数
        /// </summary>
        [ConfigurationProperty("minThreadPoolThreads", IsRequired = false, DefaultValue = 10)]
        public int MinThreadPoolThreads
        {
            get { return (int)this["minThreadPoolThreads"]; }
        }

        /// <summary>
        /// 最大连接数
        /// </summary>
        [ConfigurationProperty("maxThreadPoolThreads", IsRequired = false, DefaultValue = 100)]
        public int MaxThreadPoolThreads
        {
            get { return (int)this["maxThreadPoolThreads"]; }
        }

        /// <summary>
        /// 客户端连接超时
        /// </summary>
        [ConfigurationProperty("clientTimeout", IsRequired = false, DefaultValue = 0)]
        public int ClientTimeout
        {
            get { return (int)(this["clientTimeout"]); }
        }

        /// <summary>
        /// host
        /// </summary>
        [ConfigurationProperty("host", IsRequired = false, DefaultValue = "")]
        public string Host
        {
            get { return (string)this["host"]; }
        }

        /// <summary>
        /// Port
        /// </summary>
        [ConfigurationProperty("port", IsRequired = true)]
        public int Port
        {
            get { return (int)this["port"]; }
        }

        /// <summary>
        /// RPC服务的注册上级节点
        /// </summary>
        [ConfigurationProperty("znodeParent", IsRequired = false, DefaultValue ="")]
        public string ZnodeParent
        {
            get { return (string)this["znodeParent"]; }
        }

        /// <summary>
        /// 服务器权重
        /// </summary>
        [ConfigurationProperty("weight", IsRequired = false, DefaultValue = 1)]
        public int Weight
        {
            get { return (int)this["weight"]; }
        }
    }
}