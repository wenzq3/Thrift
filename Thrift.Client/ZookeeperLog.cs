using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thrift.Client
{
    public class ZookeeperLog : org.apache.utils.ILogConsumer
    {
        public void Log(TraceLevel severity, string className, string message, Exception exception)
        {
            if (exception == null)
                ThriftLog.Info($"Zookeeper {severity.ToString()} {className} {message} ");
            else
                ThriftLog.Info($"Zookeeper {severity.ToString()} {className} {message} {exception.Message} {exception.StackTrace}");
        }
    }
}
