using log4net;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Thrift.ServiceWin
{
    /// <summary>
    /// 日志管理类(各种方法用于记录不同级别的日志，本质上没有什么区别，只是可以结合配置中的Level决定最终是否记录该级别的日志，以达到灵活性)
    /// </summary>
    public class LogHelper
    {
        protected LogHelper()
        {
        }
        private static object lockHelper = new object();

        private static ILog _log = null;

        /// <summary>
        /// 初始化日志管理器
        /// </summary>
        private static void Init()
        {
            //配置文件路径
            FileInfo configFile = new FileInfo(string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, ConfigurationManager.AppSettings["log4netConfigPath"]));

            if (!configFile.Exists)
            {
                CreateConfigFile(configFile);
            }

            XmlConfigurator.ConfigureAndWatch(configFile);
            _log = LogManager.GetLogger("MyLogger");
        }

        /// <summary>
        /// 创建配置文件
        /// </summary>
        /// <param name="fileInfo">文件信息</param>
        private static void CreateConfigFile(FileInfo fileInfo)
        {
            using (StreamWriter sw = File.CreateText(fileInfo.FullName))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
                sw.WriteLine("<configuration>");
                sw.WriteLine("  <log4net>");
                sw.WriteLine("    <root>");
                sw.WriteLine("      <level value=\"ALL\"/>");
                sw.WriteLine("       <appender-ref ref=\"InfoAppender\" />");
                sw.WriteLine("       <appender-ref ref=\"ErrorAppender\"/>");
                sw.WriteLine("       <appender-ref ref=\"FatalAppender\"/>");
                sw.WriteLine("    </root>");

                sw.WriteLine("    <appender name=\"InfoAppender\" type=\"log4net.Appender.RollingFileAppender\">");
                sw.WriteLine("      <filter type=\"log4net.Filter.LevelRangeFilter\">");
                sw.WriteLine("          <param name=\"LevelMin\" value=\"INFO\" />");
                sw.WriteLine("          <param name=\"LevelMax\" value=\"INFO\" />");
                sw.WriteLine("      </filter>");
                sw.WriteLine("      <param name=\"File\" value=\"Logs/info_\"/>");
                sw.WriteLine("      <param name=\"AppendToFile\" value=\"true\"/>");
                sw.WriteLine("      <param name=\"RollingStyle\" value=\"Composite\"/>");
                sw.WriteLine("      <param name=\"DatePattern\" value=\"yyyy-MM-dd&quot;.log&quot;\"/>");
                sw.WriteLine("      <param name=\"MaximumFileSize\" value=\"10MB\"/>");
                sw.WriteLine("      <param name=\"MaxSizeRollBackups\" value=\"10\"/>");
                sw.WriteLine("      <param name=\"StaticLogFileName\" value=\"false\"/>");
                sw.WriteLine("      <lockingModel type=\"log4net.Appender.FileAppender+MinimalLock\" />");
                sw.WriteLine("      <layout type=\"log4net.Layout.PatternLayout\">");
                sw.WriteLine("        <param name=\"ConversionPattern\" value=\"[时间]:%d%n[级别]:%p%n[内容]:%m%n%n\"/>");
                sw.WriteLine("      </layout>");
                sw.WriteLine("    </appender>");

                sw.WriteLine("    <appender name=\"ErrorAppender\" type=\"log4net.Appender.RollingFileAppender\">");
                sw.WriteLine("      <filter type=\"log4net.Filter.LevelRangeFilter\">");
                sw.WriteLine("          <param name=\"LevelMin\" value=\"ERROR\" />");
                sw.WriteLine("          <param name=\"LevelMax\" value=\"ERROR\" />");
                sw.WriteLine("      </filter>");
                sw.WriteLine("      <param name=\"File\" value=\"Logs/error_\"/>");
                sw.WriteLine("      <param name=\"AppendToFile\" value=\"true\"/>");
                sw.WriteLine("      <param name=\"RollingStyle\" value=\"Composite\"/>");
                sw.WriteLine("      <param name=\"DatePattern\" value=\"yyyy-MM-dd&quot;.log&quot;\"/>");
                sw.WriteLine("      <param name=\"MaximumFileSize\" value=\"10MB\"/>");
                sw.WriteLine("      <param name=\"MaxSizeRollBackups\" value=\"10\"/>");
                sw.WriteLine("      <param name=\"StaticLogFileName\" value=\"false\"/>");
                sw.WriteLine("      <lockingModel type=\"log4net.Appender.FileAppender+MinimalLock\" />");
                sw.WriteLine("      <layout type=\"log4net.Layout.PatternLayout\">");
                sw.WriteLine("        <param name=\"ConversionPattern\" value=\"[时间]:%d%n[级别]:%p%n[内容]:%m%n%n\"/>");
                sw.WriteLine("      </layout>");
                sw.WriteLine("    </appender>");

                sw.WriteLine("    <appender name=\"FatalAppender\" type=\"log4net.Appender.RollingFileAppender\">");
                sw.WriteLine("      <filter type=\"log4net.Filter.LevelRangeFilter\">");
                sw.WriteLine("          <param name=\"LevelMin\" value=\"FATAL\" />");
                sw.WriteLine("          <param name=\"LevelMax\" value=\"FATAL\" />");
                sw.WriteLine("      </filter>");
                sw.WriteLine("      <param name=\"File\" value=\"Logs/fatal_\"/>");
                sw.WriteLine("      <param name=\"AppendToFile\" value=\"true\"/>");
                sw.WriteLine("      <param name=\"RollingStyle\" value=\"Composite\"/>");
                sw.WriteLine("      <param name=\"DatePattern\" value=\"yyyy-MM-dd&quot;.log&quot;\"/>");
                sw.WriteLine("      <param name=\"MaximumFileSize\" value=\"10MB\"/>");
                sw.WriteLine("      <param name=\"MaxSizeRollBackups\" value=\"10\"/>");
                sw.WriteLine("      <param name=\"StaticLogFileName\" value=\"false\"/>");
                sw.WriteLine("      <lockingModel type=\"log4net.Appender.FileAppender+MinimalLock\" />");
                sw.WriteLine("      <layout type=\"log4net.Layout.PatternLayout\">");
                sw.WriteLine("        <param name=\"ConversionPattern\" value=\"[时间]:%d%n[级别]:%p%n[内容]:%m%n%n\"/>");
                sw.WriteLine("      </layout>");
                sw.WriteLine("    </appender>");

                sw.WriteLine("  </log4net>");
                sw.WriteLine("</configuration>");
                sw.Close();
            }
        }

        private static ILog MyLog
        {
            get
            {
                if (_log == null)
                {
                    lock (lockHelper)
                    {
                        if (_log == null)
                            Init();
                    }
                }

                return _log;
            }
        }

        /// <summary>
        /// 致命的、毁灭性的(用户记录一些程序运行时出现的致命错误)
        /// </summary>
        /// <param name="message">日志摘要</param>
        public static void Fatal(string message, Exception exception = null)
        {
            MyLog.Fatal(message, exception);
        }

        /// <summary>
        /// 错误(用户记录一些程序运行时出现错误)
        /// </summary>
        /// <param name="message">日志摘要</param>
        /// <param name="exception">异常</param>
        public static void Error(string message, Exception exception = null)
        {
            MyLog.Error(message, exception);
        }

        /// <summary>
        /// 信息(用于记录一些辅助信息)
        /// </summary>
        /// <param name="message">日志摘要</param>
        /// <param name="exception">异常</param>
        public static void Info(string message, Exception exception = null, bool isTimeout = false, long millisecond = 0)
        {
            MyLog.Info(message, exception);
        }
    }
}
