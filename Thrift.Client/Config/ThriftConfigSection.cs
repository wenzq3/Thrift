using System.Configuration;

namespace Thrift.Client.Config
{
    /// <summary>
    /// thrift config section
    /// </summary>
    public class ThriftConfigSection : ConfigurationSection
    {
        public void SetSerivces(ServiceCollection services)
        {
            this.Services = services;
        }
        /// <summary>
        /// 服务集合。
        /// </summary>
        [ConfigurationProperty("services", IsRequired = true)]
        public ServiceCollection Services
        {
            get { return this["services"] as ServiceCollection; }
            set { this["services"] =value; }
        }


    }
}