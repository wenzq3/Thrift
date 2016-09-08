using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thrift.Client
{
    public class ThriftLog
    {
        public static Action<string> _eventInfo = null;
        public static Action<string> _eventError = null;
        public static void Info(string msg)
        {
            Console.WriteLine(msg);
            bool flag = ThriftLog._eventInfo != null;
            if (flag)
            {
                ThriftLog._eventInfo(msg);
            }
        }
        public static void Error(string msg)
        {
            Console.WriteLine(msg);
            bool flag = ThriftLog._eventError != null;
            if (flag)
            {
                ThriftLog._eventError(msg);
            }
        }
    }
}
