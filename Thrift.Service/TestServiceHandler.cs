using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thrift.Test;
using Thrift.Test.Thrift;

namespace Thrift.Service
{
    public class TestServiceHandler : MarshalByRefObject, ThriftTestThrift.Iface
    {
        public TestInfoThrift GetTestInfo()
        {
            System.Threading.Thread.Sleep(10);
            return new TestInfoThrift()
            {
                Status = true,
                Data = new TestInfo() { TestId = 1, TestName = "name" }
            };
        }
    }
}
