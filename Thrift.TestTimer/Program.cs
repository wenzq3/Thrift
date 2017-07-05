using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thrift.Test.Thrift;

namespace Thrift.TestTimer
{
    class Program
    {
        static void Main(string[] args)
        {
            CodeTimer.Time("newset", 1, () =>
            {
                var type = typeof(ThriftTestThrift.Client);
                string serviceName =$"serviceName_{type.Namespace}_{type.ReflectedType.Name}";
           //     Console.WriteLine(serviceName);
            });

            CodeTimer.Time("newset2", 10000, () =>
            {
                var type = typeof(ThriftTestThrift.Client);
                string serviceName = $"serviceName_{type.Namespace}_{type.ReflectedType.Name}";
                //     Console.WriteLine(serviceName);
            });

            Console.Read();
        }
    }
}
