using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thrift.Client
{
    public class ThriftLog
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(ThriftLog));

        private static bool _log = false;

        static ThriftLog()
        {
            var log = ConfigurationManager.AppSettings["ThriftLog"];
            if (log == null || log == "1")
                _log = true;
            else
                _log = false;
        }

        public static void Info(string msg)
        {
            Console.WriteLine(msg);
            if (_log)
                LOG.Info("ThriftClient:" + msg);
        }
        public static void Error(string msg)
        {
            Console.WriteLine(msg);
            if (_log)
                LOG.Error("ThriftClient:" + msg);
        }
    }
}
