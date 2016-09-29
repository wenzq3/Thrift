using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thrift.Client
{
    public class ThriftLog
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(ThriftLog));

        public static void Info(string msg)
        {
            Console.WriteLine(msg);
            LOG.Info("ThriftClient:"+msg);
        }
        public static void Error(string msg)
        {
            Console.WriteLine(msg);
            LOG.Error("ThriftClient:" + msg);
        }
    }
}
