using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Caching;

namespace Thrift.Server
{
    /// <summary>
    /// IP连接限制类
    /// </summary>
    public static class IpLimit
    {
        public static bool AllowConfig(string ip)
        {
            long longIP = IPToLong(ip);
            foreach (var item in GetInstance())
            {

                if (longIP >= item.Item1 && longIP <= item.Item2)
                    return true;
            }

            return false;
        }

        private static List<Tuple<long, long>> GetInstance()
        {
            var cache = System.Web.HttpRuntime.Cache;
            if (cache["IPLimitConfig"] == null)
            {
                string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigurationManager.AppSettings["IpLimitPath"]);
                var dep = new CacheDependency(configFilePath, DateTime.Now);

                var instance = GetConfig_Ranges(configFilePath);
                if (instance != null)
                    cache.Insert("IPLimitConfig", instance, dep, DateTime.Now.AddDays(1), TimeSpan.Zero);
                return instance;
            }
            return (List<Tuple<long, long>>)cache["IPLimitConfig"];
        }

        private static List<Tuple<long, long>> GetConfig_Ranges(string configFilePath)
        {
            var config = ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap
            {
                ExeConfigFilename = configFilePath
            }, ConfigurationUserLevel.None);

            List<Tuple<long, long>> listConfig = new List<Tuple<long, long>>();

            string strRanges = config.AppSettings.Settings["IpWhite"].Value;

            if (!string.IsNullOrEmpty(strRanges))
            {
                var ipArray = strRanges.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string range in ipArray)
                    listConfig.Add(GenerateIpRange(range));
            }

            return listConfig;
        }

        private static Tuple<long, long> GenerateIpRange(string range)
        {
            var ipArray = range.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);

            if (ipArray.Length == 0)
                throw new ArgumentException("IP限制配置错误!");

            if (ipArray.Length == 1)
                return new Tuple<long, long>(IPToLong(ipArray[0]), IPToLong(ipArray[0]));
            else
                return new Tuple<long, long>(IPToLong(ipArray[0]), IPToLong(ipArray[1]));
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="ip">IP字符串</param>
        /// <returns>long数据</returns>
        private static long IPToLong(string ip)
        {
            string[] ipArr = ip.Split(new[] { '.' });
            return int.Parse(ipArr[0]) * (long)16777216 + int.Parse(ipArr[1]) * 65536 + int.Parse(ipArr[2]) * 256 + int.Parse(ipArr[3]);
        }

        private static string LongToIP(long longIP)
        {
            StringBuilder sb = new StringBuilder("");
            sb.Append((longIP & 0xFF000000) >> 24);
            sb.Append(".");
            sb.Append((longIP & 0x00FF0000) >> 16);
            sb.Append(".");
            sb.Append((longIP & 0x0000FF00) >> 8);
            sb.Append(".");
            sb.Append(longIP & 0x000000FF);
            return sb.ToString();
        }
    }
}
